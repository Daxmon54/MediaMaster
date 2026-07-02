Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline
Imports Xunit

Public Class DisplayOrderFormatterTests

    Private Shared Function MakeTrack() As TrackInfo
        Return New TrackInfo With {.Artist = "ABBA", .Title = "Dancing Queen", .Year = "1976"}
    End Function

    <Theory>
    <InlineData(DisplayOrderMode.ArtistTitle, "ABBA - Dancing Queen")>
    <InlineData(DisplayOrderMode.ArtistTitleYear, "ABBA - Dancing Queen - 1976")>
    <InlineData(DisplayOrderMode.TitleArtist, "Dancing Queen - ABBA")>
    <InlineData(DisplayOrderMode.TitleArtistYear, "Dancing Queen - ABBA - 1976")>
    Public Sub Format_AppliesEachOfTheFourModes(mode As DisplayOrderMode, expected As String)
        Dim settings As New DisplayOrderSettings With {.Mode = mode, .Separator1 = "-", .Separator2 = "-"}

        Dim result = DisplayOrderFormatter.Format(MakeTrack(), settings)

        Assert.Equal(expected, result)
    End Sub

    <Fact>
    Public Sub Format_UsesConfiguredSeparators()
        Dim settings As New DisplayOrderSettings With {.Mode = DisplayOrderMode.ArtistTitleYear, .Separator1 = "::", .Separator2 = "//"}

        Dim result = DisplayOrderFormatter.Format(MakeTrack(), settings)

        Assert.Equal("ABBA :: Dancing Queen // 1976", result)
    End Sub

End Class
