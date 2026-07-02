Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports FluentFTP
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Polls an FTP server for "dab.txt" (a single JSON object, wrapped in [] to make it a valid
    ''' array before parsing -- matching the original's "[" &amp; sJson &amp; "]" trick) and extracts
    ''' the current artist/title from data.metadata.music[0].artists[0] (0-based; the original
    ''' WLanguage arrays are 1-based: music[1].artists[1]).
    ''' </summary>
    Public Class AcrCloudFtpMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _settings As AcrFtpSettings
        Private _cts As CancellationTokenSource
        Private _pollTask As Task
        Private _lastTrackKey As String = String.Empty

        Public Sub New(settings As AcrFtpSettings)
            _settings = settings
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return Math.Max(_settings.PollIntervalSeconds, 1) * 1000
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_settings.Host) Then
                Throw New InvalidOperationException("AcrFtpSettings.Host is not configured.")
            End If

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            _pollTask = Task.Run(AddressOf PollLoopAsync)
            Return Task.CompletedTask
        End Function

        Public Async Function StopAsync() As Task Implements ISourceMonitor.StopAsync
            _cts?.Cancel()
            If _pollTask IsNot Nothing Then
                Try
                    Await _pollTask
                Catch ex As OperationCanceledException
                End Try
            End If
            _cts?.Dispose()
        End Function

        Private Async Function PollLoopAsync() As Task
            Try
                Using timer As New PeriodicTimer(TimeSpan.FromSeconds(Math.Max(_settings.PollIntervalSeconds, 1)))
                    Do
                        Await PollOnceAsync(_cts.Token)
                    Loop While Await timer.WaitForNextTickAsync(_cts.Token)
                End Using
            Catch ex As OperationCanceledException
                ' expected during shutdown
            End Try
        End Function

        Private Async Function PollOnceAsync(cancellationToken As CancellationToken) As Task
            Try
                Using client As New AsyncFtpClient(_settings.Host, _settings.UserName, _settings.Password, 21, Nothing, Nothing)
                    Await client.Connect(cancellationToken)

                    If Not Await client.FileExists("dab.txt", cancellationToken) Then
                        Return
                    End If

                    Dim bytes = Await client.DownloadBytes("dab.txt", cancellationToken)
                    Dim rawJson = Encoding.UTF8.GetString(bytes)
                    Dim track = AcrDabTxtParser.Parse(rawJson)
                    If track Is Nothing Then
                        Return
                    End If

                    Dim trackKey = $"{track.Artist}|{track.Title}"
                    If trackKey = _lastTrackKey Then
                        Return
                    End If
                    _lastTrackKey = trackKey

                    RaiseEvent TrackChanged(Me, track)
                End Using
            Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException)
                ' network hiccups (host unreachable, auth failure, missing/malformed file, etc.)
                ' are expected for an FTP poll -- just try again next tick.
            End Try
        End Function

        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
            Return New ValueTask(StopAsync())
        End Function

    End Class

End Namespace
