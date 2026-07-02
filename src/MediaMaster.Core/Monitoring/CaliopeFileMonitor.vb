Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Watches the Caliope "now playing" file for changes and raises TrackChanged when its first
    ''' line (format "Artist - Title") differs from the last-seen line. Simplest of the 6 sources:
    ''' single file, no network, no daily-filename rollover (unlike OTS).
    ''' </summary>
    Public Class CaliopeFileMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _watchFilePath As String
        Private ReadOnly _syncLock As New Object()
        Private _watcher As FileSystemWatcher
        Private _lastLine As String = String.Empty

        Public Sub New(settings As CaliopeSettings)
            _watchFilePath = settings.WatchFile
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return 0 ' event-driven (FileSystemWatcher), not on a fixed interval
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_watchFilePath) Then
                Throw New InvalidOperationException("CaliopeSettings.WatchFile is not configured.")
            End If

            Dim watchDirectory = Path.GetDirectoryName(_watchFilePath)
            Dim watchFileName = Path.GetFileName(_watchFilePath)

            ' The external Caliope tool owns this file and may not have started writing yet
            ' (or on a fresh install, may never have run) -- create it so the watcher can still attach.
            If Not Directory.Exists(watchDirectory) Then
                Directory.CreateDirectory(watchDirectory)
            End If
            If Not File.Exists(_watchFilePath) Then
                File.WriteAllText(_watchFilePath, String.Empty)
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
                Dim line = ReadFirstLineWithRetry(e.FullPath)
                If line Is Nothing OrElse line = _lastLine Then
                    Return
                End If
                _lastLine = line

                Dim parts = line.Split({" - "}, 2, StringSplitOptions.None)
                If parts.Length <> 2 Then
                    Return
                End If

                Dim track As New TrackInfo With {
                    .Artist = parts(0).Trim(),
                    .Title = parts(1).Trim(),
                    .InfoType = 1
                }

                If String.IsNullOrEmpty(track.Artist) OrElse String.IsNullOrEmpty(track.Title) Then
                    Return
                End If

                RaiseEvent TrackChanged(Me, track)
            End SyncLock
        End Sub

        ''' <summary>The writer may still hold the file open briefly after the Changed event fires; retry a few times.</summary>
        Private Shared Function ReadFirstLineWithRetry(path As String) As String
            For attempt = 1 To 5
                Try
                    Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        Using reader As New StreamReader(stream)
                            Return reader.ReadLine()
                        End Using
                    End Using
                Catch ex As IOException
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
