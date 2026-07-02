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
    ''' WIN_Source's folder-picker button for this field actually uses fSelect (a single-file
    ''' picker filtered to *.xml), the same pattern as Caliope's fSelect (*.txt) -- confirming
    ''' this was always a specific watch *file*, not a folder, despite the "WatchFolder" naming
    ''' in the original globals. This watches that one file directly, same as CaliopeFileMonitor,
    ''' rather than scanning a folder for any *.xml change.
    '''
    ''' Also fixes original bug #2: DataOntvangst4() loaded XML from the configured path directly
    ''' instead of the file path handed to it by the tracking callback -- reading the
    ''' FileSystemWatcher event's own FullPath here is the more robust equivalent.
    ''' </summary>
    Public Class PowerStudioFileMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _watchFilePath As String
        Private ReadOnly _syncLock As New Object()
        Private _watcher As FileSystemWatcher
        Private _lastArtist As String = String.Empty
        Private _lastTitle As String = String.Empty

        Public Sub New(settings As PowerStudioSettings)
            _watchFilePath = settings.WatchFile
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return 0
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_watchFilePath) Then
                Throw New InvalidOperationException("PowerStudioSettings.WatchFile is not configured.")
            End If

            Dim watchDirectory = Path.GetDirectoryName(_watchFilePath)
            Dim watchFileName = Path.GetFileName(_watchFilePath)

            ' The external PowerStudio tool owns this file and may not have started writing yet
            ' (or on a fresh install, may never have run) -- create it so the watcher can still attach.
            If Not Directory.Exists(watchDirectory) Then
                Directory.CreateDirectory(watchDirectory)
            End If
            If Not File.Exists(_watchFilePath) Then
                File.WriteAllText(_watchFilePath, "<BroadcastMonitor><Current><artistName /><titleName /></Current></BroadcastMonitor>")
            End If

            _watcher = New FileSystemWatcher(watchDirectory, watchFileName) With {
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
