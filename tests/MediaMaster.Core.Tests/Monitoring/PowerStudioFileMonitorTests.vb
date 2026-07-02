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
        ' WIN_Source's picker for this field uses a single-file selector (fSelect, *.xml filter),
        ' the same pattern as Caliope -- confirming this watches one specific file, not a folder.
        Dim xmlFile = Path.Combine(_tempDirectory, "nowplaying.xml")
        File.WriteAllText(xmlFile, BuildXml("", ""))

        ' A decoy file sits in the same folder -- the monitor must not react to it changing,
        ' proving it watches only the specific configured file.
        Dim decoyFile = Path.Combine(_tempDirectory, "decoy.xml")
        File.WriteAllText(decoyFile, BuildXml("", ""))

        Dim settings As New PowerStudioSettings With {.WatchFile = xmlFile}
        Dim monitor As New PowerStudioFileMonitor(settings)
        Await monitor.StartAsync(CancellationToken.None)

        Dim decoyTrack = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(decoyFile, BuildXml("WRONG ARTIST", "WRONG TITLE")), timeoutSeconds:=2)
        Assert.Null(decoyTrack)

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

        Dim settings As New PowerStudioSettings With {.WatchFile = xmlFile}
        Dim monitor As New PowerStudioFileMonitor(settings)
        Await monitor.StartAsync(CancellationToken.None)

        Dim firstTrack = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen")))
        Assert.NotNull(firstTrack)

        Dim secondTrack = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(xmlFile, BuildXml("ABBA", "Dancing Queen")), timeoutSeconds:=2)
        Assert.Null(secondTrack)

        Await monitor.StopAsync()
    End Function

End Class
