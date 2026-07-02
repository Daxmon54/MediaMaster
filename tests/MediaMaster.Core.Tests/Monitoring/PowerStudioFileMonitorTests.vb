Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class PowerStudioFileMonitorTests
    Implements IDisposable

    Private ReadOnly _tempDirectory As String

    Public Sub New()
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mm-powerstudio-{Guid.NewGuid()}")
        Directory.CreateDirectory(_tempDirectory)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            Directory.Delete(_tempDirectory, recursive:=True)
        Catch
        End Try
    End Sub

    Private Shared Async Function WaitForTrackChangedAsync(monitor As PowerStudioFileMonitor, act As Action, Optional timeoutSeconds As Integer = 10) As Task(Of TrackInfo)
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

    Private Shared Function BuildXml(artist As String, title As String) As String
        Return $"<BroadcastMonitor><Current><artistName>{artist}</artistName><titleName>{title}</titleName></Current></BroadcastMonitor>"
    End Function

    <Fact>
    Public Async Function RaisesTrackChanged_WithParsedArtistAndTitle_FromTheChangedFile() As Task
        ' Regression for original bug #2: DataOntvangst4() read from the watched *folder* path
        ' instead of the specific file that changed. Here the monitor must be able to correctly
        ' read a file that changed, proving it uses the file path from the watcher event.
        Dim xmlFile = Path.Combine(_tempDirectory, "nowplaying.xml")
        File.WriteAllText(xmlFile, BuildXml("", ""))

        ' A decoy file sits in the same folder with different (stale) content -- if the monitor
        ' mistakenly read "the folder" instead of the specific changed file, it could pick this up.
        File.WriteAllText(Path.Combine(_tempDirectory, "decoy.xml"), BuildXml("WRONG ARTIST", "WRONG TITLE"))

        Dim settings As New PowerStudioSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New PowerStudioFileMonitor(settings)
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen")))

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function DoesNotRaiseTrackChanged_WhenArtistAndTitleAreUnchanged() As Task
        Dim xmlFile = Path.Combine(_tempDirectory, "nowplaying.xml")
        File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen"))

        Dim settings As New PowerStudioSettings With {.WatchFolder = _tempDirectory}
        Dim monitor As New PowerStudioFileMonitor(settings)
        Await monitor.StartAsync(CancellationToken.None)

        Dim firstTrack = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen")))
        Assert.NotNull(firstTrack)

        Dim secondTrack = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen")), timeoutSeconds:=2)
        Assert.Null(secondTrack)

        Await monitor.StopAsync()
    End Function

End Class
