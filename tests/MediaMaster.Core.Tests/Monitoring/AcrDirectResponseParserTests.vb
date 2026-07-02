Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class AcrDirectResponseParserTests

    Private Shared Function BuildJson(artist As String, title As String) As String
        Return $"
        {{
          ""data"": {{
            ""metadata"": {{
              ""music"": [
                {{
                  ""title"": ""{title}"",
                  ""artists"": [ {{ ""name"": ""{artist}"" }} ],
                  ""label"": ""Polar"",
                  ""album"": {{ ""name"": ""Arrival"" }},
                  ""external_ids"": {{ ""isrc"": [ ""SEZ037700477"" ] }},
                  ""release_date"": ""1976-08-15""
                }}
              ]
            }}
          }}
        }}"
    End Function

    <Fact>
    Public Sub Parse_ExtractsAllFields()
        Dim track = AcrDirectResponseParser.Parse(BuildJson("ABBA", "Dancing Queen"))

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)
        Assert.Equal("Polar", track.Label)
        Assert.Equal("Arrival", track.Album)
        Assert.Equal("SEZ037700477", track.Isrc)
        Assert.Equal("1976", track.Year)
        Assert.Equal(1, track.InfoType)
    End Sub

    <Fact>
    Public Sub Parse_ReturnsNothing_WhenArtistAndTitleConcatenateToSentinel()
        Dim track = AcrDirectResponseParser.Parse(BuildJson("0", "0"))

        Assert.Null(track)
    End Sub

    <Fact>
    Public Sub Parse_ReturnsNothing_ForMalformedJson()
        Assert.Null(AcrDirectResponseParser.Parse("not json"))
        Assert.Null(AcrDirectResponseParser.Parse(""))
    End Sub

End Class
