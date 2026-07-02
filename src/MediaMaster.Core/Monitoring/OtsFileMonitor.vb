Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Watches OTS's daily "YYYY-MM-DD-playlog.txt" file for changes. Rather than replicating the
    ''' original's watch-one-specific-file-and-restart-at-midnight approach (StartTracking/Klok's
    ''' minute=0 check), this watches the whole folder and simply ignores changes to any file that
    ''' isn't today's -- functionally equivalent, without needing a separate midnight-restart service
    ''' or restart race conditions right at rollover.
    ''' </summary>
    Public Class OtsFileMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _watchFolder As String
        Private ReadOnly _pflFolder As String
        Private ReadOnly _syncLock As New Object()
        Private _watcher As FileSystemWatcher
        Private _lastLine As String = String.Empty

        Public Sub New(settings As OtsSettings, pflFolder As String)
            _watchFolder = settings.WatchFolder
            _pflFolder = pflFolder
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return 0
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_watchFolder) Then
                Throw New InvalidOperationException("OtsSettings.WatchFolder is not configured.")
            End If
            If Not Directory.Exists(_watchFolder) Then
                Directory.CreateDirectory(_watchFolder)
            End If

            _watcher = New FileSystemWatcher(_watchFolder, "*-playlog.txt") With {
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

        Private Shared Function TodaysFileName() As String
            Return $"{DateTime.Now:yyyy-MM-dd}-playlog.txt"
        End Function

        Private Sub OnFileChanged(sender As Object, e As FileSystemEventArgs)
            If Not String.Equals(Path.GetFileName(e.FullPath), TodaysFileName(), StringComparison.OrdinalIgnoreCase) Then
                Return
            End If

            If PflGate.IsSuppressed(_pflFolder) Then
                Return
            End If

            SyncLock _syncLock
                Dim lastLine = ReadLastLineWithRetry(e.FullPath)
                If lastLine Is Nothing OrElse lastLine = _lastLine Then
                    Return
                End If
                _lastLine = lastLine

                ' Original strips a fixed-width 21-character leading "date time " prefix that OTS writes before the track.
                If lastLine.Length <= 21 Then
                    Return
                End If
                Dim payload = lastLine.Substring(21)

                If payload.Contains("[Unknown]") Then
                    Return
                End If

                Dim parts = payload.Split({" - "}, 2, StringSplitOptions.None)
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

        Private Shared Function ReadLastLineWithRetry(path As String) As String
            For attempt = 1 To 5
                Try
                    Dim lines = File.ReadAllLines(path)
                    Return If(lines.Length > 0, lines(lines.Length - 1), Nothing)
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
