Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Listens on a local UDP socket for Carmen automation messages ("title;artist;year;eventType")
    ''' and raises TrackChanged for song events (eventType 1) and, in "all info" mode, for a handful
    ''' of other event slots mapped to configured description text (time signal, commercial, info,
    ''' and 10 generic event slots) -- mirroring DataOntvangst()'s type dispatch.
    '''
    ''' Note: the original also creates a second ("remote port") socket in commented-out code that
    ''' was never actually active -- only the local-port listener is real, so only that is ported.
    ''' </summary>
    Public Class CarmenUdpMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _settings As CarmenSettings
        Private ReadOnly _descriptions As DescriptionSettings
        Private ReadOnly _pflFolder As String
        Private _udpClient As UdpClient
        Private _cts As CancellationTokenSource
        Private _listenTask As Task

        Public Sub New(settings As CarmenSettings, descriptions As DescriptionSettings, pflFolder As String)
            _settings = settings
            _descriptions = descriptions
            _pflFolder = pflFolder
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return 0 ' event-driven UDP listener
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If _settings.LocalPort <= 0 Then
                Throw New InvalidOperationException("CarmenSettings.LocalPort is not configured.")
            End If

            _udpClient = New UdpClient(_settings.LocalPort)
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            _listenTask = Task.Run(AddressOf ListenLoopAsync)

            Return Task.CompletedTask
        End Function

        Public Async Function StopAsync() As Task Implements ISourceMonitor.StopAsync
            If _cts IsNot Nothing Then
                _cts.Cancel()
            End If
            _udpClient?.Close()

            If _listenTask IsNot Nothing Then
                Try
                    Await _listenTask
                Catch ex As OperationCanceledException
                Catch ex As ObjectDisposedException
                End Try
            End If

            _udpClient?.Dispose()
            _cts?.Dispose()
        End Function

        Private Async Function ListenLoopAsync() As Task
            While Not _cts.IsCancellationRequested
                Try
                    Dim result = Await _udpClient.ReceiveAsync(_cts.Token)
                    Dim text = Encoding.UTF8.GetString(result.Buffer)
                    If Not String.IsNullOrEmpty(text) Then
                        ProcessMessage(text)
                    End If
                Catch ex As OperationCanceledException
                    Exit While
                Catch ex As ObjectDisposedException
                    Exit While
                Catch ex As SocketException
                    Exit While
                End Try
            End While
        End Function

        Private Sub ProcessMessage(rawMessage As String)
            If PflGate.IsSuppressed(_pflFolder) Then
                Return
            End If

            Dim fields = rawMessage.Split(";"c)
            If fields.Length < 4 Then
                Return
            End If

            Dim title = fields(0)
            Dim artist = fields(1)
            Dim yearField = fields(2)
            Dim infoType As Integer
            Integer.TryParse(fields(3), infoType)

            ' Mode 1 ("all info") skips Carmen's own filler placeholder tracks; mode 2 doesn't check this
            ' (an inconsistency present in the original too).
            If _settings.Mode = 1 AndAlso infoType = 1 AndAlso artist.Contains("Audio Item", StringComparison.OrdinalIgnoreCase) Then
                Return
            End If

            Dim track As TrackInfo = Nothing
            If infoType = 1 Then
                Dim yearNumber As Integer
                Integer.TryParse(yearField, yearNumber)
                track = New TrackInfo With {.Artist = artist, .Title = title, .Year = yearNumber.ToString(), .InfoType = infoType}
            ElseIf _settings.Mode = 1 Then
                ' Mode 1 also dispatches non-song event types to their configured description text; mode 2 ("song only") ignores them entirely.
                track = New TrackInfo With {.Artist = ResolveEventArtist(infoType), .Title = String.Empty, .InfoType = infoType}
            End If

            If track Is Nothing Then
                Return
            End If

            track.Artist = If(track.Artist, String.Empty).Trim()
            track.Title = If(track.Title, String.Empty).Trim()
            If String.IsNullOrEmpty(track.Artist) AndAlso String.IsNullOrEmpty(track.Title) Then
                Return
            End If

            RaiseEvent TrackChanged(Me, track)
        End Sub

        Private Function ResolveEventArtist(infoType As Integer) As String
            Select Case infoType
                Case 5
                    Return _descriptions.TimeOfTheHour
                Case 10, 11, 12, 13, 14, 15
                    Return _descriptions.Commercial
                Case 20, 21, 22, 23, 24, 25
                    Return _descriptions.Info
                Case 30 To 39
                    Dim index = infoType - 30
                    If index >= 0 AndAlso index < _descriptions.Events.Length Then
                        Return _descriptions.Events(index)
                    End If
                    Return String.Empty
                Case Else
                    Return String.Empty
            End Select
        End Function

        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
            Return New ValueTask(StopAsync())
        End Function

    End Class

End Namespace
