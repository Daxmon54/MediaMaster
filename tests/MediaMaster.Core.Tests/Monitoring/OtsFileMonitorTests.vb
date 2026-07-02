Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class OtsFileMonitorTests
    Implements IDisposable

    Private ReadOnly _tempDirectory As String

    Public Sub New()
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mm-ots-{Guid.NewGuid()}")
        Directory.CreateDirectory(_tempDirectory)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            Directory.Delete(_tempDirectory, recursive:=True)
        Catch
        End Try
    End Sub

    Private Shared Async Function WaitForTrackChangedAsync(monitor As OtsFileMonitor, act As Action, Optional timeoutSeconds As Integer = 10) As Task(Of TrackInfo)
        Dim completionSource As New TaskCompletionSource(Of TrackInfo)()
        Dim handler As EventHandler(Of TrackInfo) = Sub(sender, track) completionSource.TrySetResult(track)
        AddHandler monitor.TrackChanged, handler
        Try
            act()
            Dim completed = Await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)))
            If completed IsNot completionSource.Task Then
                Return Nothing
            End If
            Return Await completionSource.Task
        Finally
            RemoveHandler monitor.TrackChanged, handler
        End Try
    End Function

    Private Shared Function BuildOtsLine(artist As String, title As String) As String
        ' Original strips a fixed 21-char "date time " prefix; any 21 characters work here.
        Return "2026-07-02 21:38:35   " & $"{artist} - {title}"
    End Function

    <Fact>
    Public Async Function RaisesTrackChanged_ForTodaysFile_WithPrefixStripped() As Task
        Dim todaysFile = Path.Combine(_tempDirectory, $"{DateTime.Now:yyyy-MM-dd}-playlog.txt")
        File.WriteAllText(todaysFile, "")

        Dim settings As New OtsSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New OtsFileMonitor(settings, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(todaysFile, BuildOtsLine("ABBA", "Dancing Queen")))

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function IgnoresChanges_ToFilesOtherThanTodays() As Task
        Dim yesterdaysFile = Path.Combine(_tempDirectory, $"{DateTime.Now.AddDays(-1):yyyy-MM-dd}-playlog.txt")
        File.WriteAllText(yesterdaysFile, "")

        Dim settings As New OtsSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New OtsFileMonitor(settings, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(yesterdaysFile, BuildOtsLine("ABBA", "Dancing Queen")), timeoutSeconds:=2)

        Assert.Null(track)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function SkipsUnknownTracks() As Task
        Dim todaysFile = Path.Combine(_tempDirectory, $"{DateTime.Now:yyyy-MM-dd}-playlog.txt")
        File.WriteAllText(todaysFile, "")

        Dim settings As New OtsSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New OtsFileMonitor(settings, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(todaysFile, "2026-07-02 21:38:35   [Unknown] - [Unknown]"), timeoutSeconds:=2)

        Assert.Null(track)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function SuppressesPublishing_WhenPflSemaphorePresent() As Task
        Dim todaysFile = Path.Combine(_tempDirectory, $"{DateTime.Now:yyyy-MM-dd}-playlog.txt")
        File.WriteAllText(todaysFile, "")

        Dim pflFolder = Path.Combine(_tempDirectory, "pfl")
        Directory.CreateDirectory(pflFolder)
        File.WriteAllText(Path.Combine(pflFolder, "PFL_A.SEM"), "")

        Dim settings As New OtsSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New OtsFileMonitor(settings, pflFolder)
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(todaysFile, BuildOtsLine("ABBA", "Dancing Queen")), timeoutSeconds:=2)

        Assert.Null(track)

        Await monitor.StopAsync()
    End Function

End Class
