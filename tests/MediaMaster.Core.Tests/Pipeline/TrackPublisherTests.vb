Imports System.Threading
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline
Imports MediaMaster.Core.Polling
Imports Moq
Imports Xunit

Public Class TrackPublisherTests

    Private Shared Function MakeSettings() As AppSettings
        Return New AppSettings With {
            .DisplayOrder = New DisplayOrderSettings With {.Mode = DisplayOrderMode.ArtistTitle, .Separator1 = "-"},
            .Export = New ExportSettings With {.Path = "", .ExtendedFileName = "ExtendedLivetrack.txt"},
            .Tracklog = New TracklogSettings With {.LogToDatabase = True, .LogToFile = False},
            .LiveTrack = New LiveTrackSettings With {.Enabled = False},
            .EditionId = 3
        }
    End Function

    ''' <summary>Builds a TrackPublisher whose IExportFileWriter appends every published track to <paramref name="capturedExports"/>.</summary>
    Private Shared Function MakePublisher(settings As AppSettings,
                                           capturedExports As List(Of TrackInfo),
                                           Optional trackLogSink As ITrackLogSink = Nothing) As TrackPublisher
        Dim settingsProvider = New Mock(Of IAppSettingsProvider)()
        settingsProvider.Setup(Function(s) s.GetSettings()).Returns(settings)

        Dim exportWriter = New Mock(Of IExportFileWriter)()
        exportWriter.Setup(Sub(w) w.Write(It.IsAny(Of TrackInfo), It.IsAny(Of ExportSettings))).
            Callback(Of TrackInfo, ExportSettings)(Sub(t, s) capturedExports.Add(t))

        Dim playlogWriter = New Mock(Of IPlaylogWriter)()
        Dim liveTrackWriter = New Mock(Of ILiveTrackWriter)()
        Dim uiSink = New Mock(Of IUiUpdateSink)()

        Return New TrackPublisher(settingsProvider.Object, exportWriter.Object, playlogWriter.Object, liveTrackWriter.Object, uiSink.Object, trackLogSink)
    End Function

    <Fact>
    Public Async Function PublishAsync_KeepsTheParsedArtistAndTitle_NotBlank() As Task
        ' Regression for original bug: CreateFile() re-derived Artist/Title from two locals that
        ' were declared but never assigned, silently blanking both for Carmen/ACR-FTP. Here the
        ' actually-parsed values must survive all the way to the export.
        Dim settings = MakeSettings()
        Dim capturedExports As New List(Of TrackInfo)()
        Dim publisher = MakePublisher(settings, capturedExports)

        Dim raw As New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
        Dim result = Await publisher.PublishAsync("CARMEN", raw, logToDatabase:=False, cancellationToken:=CancellationToken.None)

        Assert.Equal("ABBA", result.Artist)
        Assert.Equal("Dancing Queen", result.Title)
        Assert.False(String.IsNullOrEmpty(result.Artist))
        Assert.False(String.IsNullOrEmpty(result.Title))
    End Function

    <Fact>
    Public Async Function PublishAsync_WritesTheSameArtistAndTitleToTheExportFile() As Task
        Dim settings = MakeSettings()
        Dim capturedExports As New List(Of TrackInfo)()
        Dim publisher = MakePublisher(settings, capturedExports)

        Dim raw As New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
        Await publisher.PublishAsync("ACR", raw, logToDatabase:=False, cancellationToken:=CancellationToken.None)

        Dim exportedTrack = Assert.Single(capturedExports)
        Assert.Equal("ABBA", exportedTrack.Artist)
        Assert.Equal("Dancing Queen", exportedTrack.Title)
    End Function

    <Fact>
    Public Async Function PublishAsync_AppliesDisplayOrderToCombinedField() As Task
        ' Regression for original bug: the display-order setting was computed but discarded.
        Dim settings = MakeSettings()
        settings.DisplayOrder = New DisplayOrderSettings With {.Mode = DisplayOrderMode.TitleArtistYear, .Separator1 = "-", .Separator2 = "-"}
        Dim capturedExports As New List(Of TrackInfo)()
        Dim publisher = MakePublisher(settings, capturedExports)

        Dim raw As New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
        Dim result = Await publisher.PublishAsync("ACR", raw, logToDatabase:=False, cancellationToken:=CancellationToken.None)

        Assert.Equal("Dancing Queen - ABBA - 1976", result.Combined)
    End Function

    <Fact>
    Public Async Function PublishAsync_SkipsPublishing_WhenArtistAndTitleAreBothEmpty() As Task
        Dim settings = MakeSettings()
        Dim capturedExports As New List(Of TrackInfo)()
        Dim publisher = MakePublisher(settings, capturedExports)

        Dim raw As New TrackInfo With {.Artist = "", .Title = ""}
        Await publisher.PublishAsync("OTS", raw, logToDatabase:=False, cancellationToken:=CancellationToken.None)

        Assert.Empty(capturedExports)
    End Function

    <Fact>
    Public Async Function PublishAsync_LogsToDatabase_OnlyWhenRequestedAndSettingEnabled() As Task
        Dim settings = MakeSettings()
        settings.Tracklog.LogToDatabase = True
        Dim capturedExports As New List(Of TrackInfo)()
        Dim trackLogSink = New Mock(Of ITrackLogSink)()
        Dim publisher = MakePublisher(settings, capturedExports, trackLogSink.Object)

        Dim raw As New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
        Await publisher.PublishAsync("ACR", raw, logToDatabase:=True, cancellationToken:=CancellationToken.None)

        trackLogSink.Verify(Function(s) s.LogAsync(It.IsAny(Of TrackInfo), 3, It.IsAny(Of CancellationToken)), Times.Once)
    End Function

    <Fact>
    Public Async Function PublishAsync_DoesNotLogToDatabase_WhenCallerDidNotRequestIt() As Task
        Dim settings = MakeSettings()
        settings.Tracklog.LogToDatabase = True
        Dim capturedExports As New List(Of TrackInfo)()
        Dim trackLogSink = New Mock(Of ITrackLogSink)()
        Dim publisher = MakePublisher(settings, capturedExports, trackLogSink.Object)

        Dim raw As New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
        Await publisher.PublishAsync("CARMEN", raw, logToDatabase:=False, cancellationToken:=CancellationToken.None)

        trackLogSink.Verify(Function(s) s.LogAsync(It.IsAny(Of TrackInfo), It.IsAny(Of Integer), It.IsAny(Of CancellationToken)), Times.Never)
    End Function

End Class
