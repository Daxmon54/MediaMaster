Imports MediaMaster.Core.Enrichment
Imports Xunit

Public Class MusicBrainzResponseParserTests

    Private Shared Function BuildJson(recordingTitle As String, artistName As String, Optional score As Integer = 100) As String
        Return $"
        {{
          ""recordings"": [
            {{
              ""score"": {score},
              ""title"": ""{recordingTitle}"",
              ""artist-credit"": [ {{ ""name"": ""{artistName}"" }} ]
            }}
          ]
        }}"
    End Function

    <Fact>
    Public Sub FindCanonical_ReturnsCanonicalCasing_WhenNormalizedInputMatches()
        Dim json = BuildJson("Dancing Queen", "ABBA")

        Dim result = MusicBrainzResponseParser.FindCanonical(json, "ABBA", "DANCING QUEEN")

        Assert.NotNull(result)
        Assert.True(result.Matched)
        Assert.Equal("ABBA", result.Artist)
        Assert.Equal("Dancing Queen", result.Title)
    End Sub

    <Fact>
    Public Sub FindCanonical_MatchesIgnoringPunctuation()
        ' Input "AC DC" should still match the canonical "AC/DC" since normalization strips non-alphanumerics.
        Dim json = BuildJson("Highway to Hell", "AC/DC")

        Dim result = MusicBrainzResponseParser.FindCanonical(json, "AC DC", "HIGHWAY TO HELL")

        Assert.NotNull(result)
        Assert.Equal("AC/DC", result.Artist)
        Assert.Equal("Highway to Hell", result.Title)
    End Sub

    <Fact>
    Public Sub FindCanonical_ReturnsNothing_WhenTopResultIsADifferentTrack()
        ' A fuzzy hit for a different song must NOT be substituted.
        Dim json = BuildJson("Waterloo", "ABBA")

        Dim result = MusicBrainzResponseParser.FindCanonical(json, "ABBA", "DANCING QUEEN")

        Assert.Null(result)
    End Sub

    <Fact>
    Public Sub FindCanonical_ReconstructsCollaborationArtistWithJoinphrase()
        Dim json = "
        {
          ""recordings"": [
            {
              ""title"": ""Under Pressure"",
              ""artist-credit"": [
                { ""name"": ""Queen"", ""joinphrase"": "" & "" },
                { ""name"": ""David Bowie"" }
              ]
            }
          ]
        }"

        Dim result = MusicBrainzResponseParser.FindCanonical(json, "QUEEN & DAVID BOWIE", "UNDER PRESSURE")

        Assert.NotNull(result)
        Assert.Equal("Queen & David Bowie", result.Artist)
        Assert.Equal("Under Pressure", result.Title)
    End Sub

    <Fact>
    Public Sub FindCanonical_ReturnsNothing_ForEmptyOrMalformed()
        Assert.Null(MusicBrainzResponseParser.FindCanonical("", "ABBA", "DANCING QUEEN"))
        Assert.Null(MusicBrainzResponseParser.FindCanonical("not json", "ABBA", "DANCING QUEEN"))
        Assert.Null(MusicBrainzResponseParser.FindCanonical("{}", "ABBA", "DANCING QUEEN"))
    End Sub

End Class
