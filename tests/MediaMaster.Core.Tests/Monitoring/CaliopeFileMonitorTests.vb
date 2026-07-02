Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline
Imports MediaMaster.Core.Polling
Imports Moq
Imports Xunit

Public Class CaliopeFileMonitorTests
    Implements IDisposable

    Private ReadOnly _tempDirectory As String

    Public Sub New()
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"mm-caliope-{Guid.NewGuid()}")
        Directory.CreateDirectory(_tempDirectory)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            Directory.Delete(_tempDirectory, recursive:=True)
        Catch
            ' best-effort cleanup
        End Try
    End Sub

    Private Shared Async Function WaitForTrackChangedAsync(monitor As CaliopeFileMonitor, act As Action) As Task(Of TrackInfo)
        Dim completionSource As New TaskCompletionSource(Of TrackInfo)()
        Dim handler As EventHandler(Of TrackInfo) = Sub(sender, track) completionSource.TrySetResult(track)
        AddHandler monitor.TrackChanged, handler
        Try
            act()
            Dim completed = Await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(10)))
            If completed IsNot completionSource.Task Then
                Throw New TimeoutException("Timed out waiting for CaliopeFileMonitor.TrackChanged.")
            End If
            Return Await completionSource.Task
        Finally
            RemoveHandler monitor.TrackChanged, handler
        End Try
    End Function

    <Fact>
    Public Async Function RaisesTrackChanged_WithParsedArtistAndTitle_WhenWatchedFileChanges() As Task
        Dim watchFile = Path.Combine(_tempDirectory, "nowplaying.txt")
        File.WriteAllText(watchFile, "")

        Dim settings As New CaliopeSettings With {.WatchFile = watchFile}
        Dim monitor As New CaliopeFileMonitor(settings)
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Sub() File.WriteAllText(watchFile, "ABBA - Dancing Queen"))

        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function EndToEnd_MonitorToPublisher_WritesExportJsonWithCorrectArtistAndTitle() As Task
        Dim watchFile = Path.Combine(_tempDirectory, "nowplaying.txt")
        File.WriteAllText(watchFile, "")
        Dim exportDir = Path.Combine(_tempDirectory, "export")

        Dim appSettings As New AppSettings With {
            .Caliope = New CaliopeSettings With {.WatchFile = watchFile},
            .Export = New ExportSettings With {.Path = exportDir, .ExtendedFileName = "ExtendedLivetrack.txt"},
            .DisplayOrder = New DisplayOrderSettings With {.Mode = DisplayOrderMode.ArtistTitle, .Separator1 = "-"}
        }
        Dim settingsProvider = New Mock(Of IAppSettingsProvider)()
        settingsProvider.Setup(Function(s) s.GetSettings()).Returns(appSettings)
        Dim uiSink = New Mock(Of IUiUpdateSink)()

        Dim publisher As New TrackPublisher(settingsProvider.Object, New ExportFileWriter(), New PlaylogWriter(), New LiveTrackWriter(), uiSink.Object)
        Dim monitor As New CaliopeFileMonitor(appSettings.Caliope)

        Dim publishCompletion As New TaskCompletionSource(Of Boolean)()
        Dim handler As EventHandler(Of TrackInfo) =
            Async Sub(sender, track)
                Await publisher.PublishAsync("Caliope", track, logToDatabase:=False, cancellationToken:=CancellationToken.None)
                publishCompletion.TrySetResult(True)
            End Sub
        AddHandler monitor.TrackChanged, handler

        Await monitor.StartAsync(CancellationToken.None)
        File.WriteAllText(watchFile, "ABBA - Dancing Queen")

        Dim completed = Await Task.WhenAny(publishCompletion.Task, Task.Delay(TimeSpan.FromSeconds(10)))
        Assert.Same(publishCompletion.Task, completed)

        Await monitor.StopAsync()

        Dim exportFile = Path.Combine(exportDir, "ExtendedLivetrack.txt")
        Assert.True(File.Exists(exportFile))
        Dim json = File.ReadAllText(exportFile)
        Assert.Contains("ABBA", json)
        Assert.Contains("Dancing Queen", json)
    End Function

End Class
