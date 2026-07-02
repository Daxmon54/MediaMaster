Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml.Linq
Imports System.Xml.XPath
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Watches a PowerStudio "now playing" XML file for changes and parses
    ''' /BroadcastMonitor/Current/artistName and /titleName.
    '''
    ''' Fixes original bug #2: DataOntvangst4() loaded XML from the *watched folder path*
    ''' (gsPowerStudio_WatchFolder) instead of the *specific file that changed*
    ''' (sFullNameOfTheTrackedFile, the parameter it was actually handed) -- here the
    ''' FileSystemWatcher event's own FullPath is what gets read.
    ''' </summary>
    Public Class PowerStudioFileMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _watchFolder As String
        Private ReadOnly _syncLock As New Object()
        Private _watcher As FileSystemWatcher
        Private _lastArtist As String = String.Empty
        Private _lastTitle As String = String.Empty

        Public Sub New(settings As PowerStudioSettings)
            _watchFolder = settings.WatchFolder
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return 0
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_watchFolder) Then
                Throw New InvalidOperationException("PowerStudioSettings.WatchFolder is not configured.")
            End If
            If Not Directory.Exists(_watchFolder) Then
                Directory.CreateDirectory(_watchFolder)
            End If

            _watcher = New FileSystemWatcher(_watchFolder, "*.xml") With {
                .NotifyFilter = NotifyFilters.LastWrite Or NotifyFilters.Size
            }
            AddHandler _watcher.Changed, AddressOf OnFileChanged
            _watcher.EnableRaisingEvents = True

            Return Task.CompletedTask
        End Function

        Public Function StopAsync() As Task Implements ISourceMonitor.StopAsync
            If _watcher IsNot Nothing Then
                _watcher.EnableRaisingEvents = False
                RemoveHandler _watcher.Changed, AddressOf OnFileChanged
                _watcher.Dispose()
                _watcher = Nothing
            End If
            Return Task.CompletedTask
        End Function

        Private Sub OnFileChanged(sender As Object, e As FileSystemEventArgs)
            SyncLock _syncLock
                ' Bug #2 fix: read e.FullPath (the file that actually changed), not _watchFolder.
                Dim document = LoadXmlWithRetry(e.FullPath)
                If document Is Nothing Then
                    Return
                End If

                Dim artist = document.XPathSelectElement("/BroadcastMonitor/Current/artistName")?.Value
                Dim title = document.XPathSelectElement("/BroadcastMonitor/Current/titleName")?.Value

                If artist = _lastArtist AndAlso title = _lastTitle Then
                    Return
                End If
                _lastArtist = artist
                _lastTitle = title

                Dim track As New TrackInfo With {
                    .Artist = If(artist, String.Empty).Trim(),
                    .Title = If(title, String.Empty).Trim(),
                    .InfoType = 1
                }

                If String.IsNullOrEmpty(track.Artist) OrElse String.IsNullOrEmpty(track.Title) Then
                    Return
                End If

                RaiseEvent TrackChanged(Me, track)
            End SyncLock
        End Sub

        Private Shared Function LoadXmlWithRetry(path As String) As XDocument
            For attempt = 1 To 5
                Try
                    Return XDocument.Load(path)
                Catch ex As IOException
                    Thread.Sleep(50)
                Catch ex As System.Xml.XmlException
                    ' the writer may have only partially flushed the file; retry
                    Thread.Sleep(50)
                End Try
            Next
            Return Nothing
        End Function

        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
            Return New ValueTask(StopAsync())
        End Function

    End Class

End Namespace
