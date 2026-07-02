Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class AcrDabTxtParserTests

    <Fact>
    Public Sub Parse_ExtractsArtistAndTitle_FromFirstMusicAndArtistEntry()
        Dim json = "
        {
          ""data"": {
            ""metadata"": {
              ""music"": [
                { ""title"": ""Dancing Queen"", ""artists"": [ { ""name"": ""ABBA"" } ] }
              ]
            }
          }
        }"

        Dim track = AcrDabTxtParser.Parse(json)

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)
        Assert.Equal(101, track.InfoType)
    End Sub

    <Theory>
    <InlineData("0", "Dancing Queen")>
    <InlineData("ABBA", "0")>
    Public Sub Parse_ReturnsNothing_WhenArtistOrTitleIsSentinelZero(artist As String, title As String)
        Dim json = $"
        {{
          ""data"": {{
            ""metadata"": {{
              ""music"": [
                {{ ""title"": ""{title}"", ""artists"": [ {{ ""name"": ""{artist}"" }} ] }}
              ]
            }}
          }}
        }}"

        Dim track = AcrDabTxtParser.Parse(json)

        Assert.Null(track)
    End Sub

    <Fact>
    Public Sub Parse_ReturnsNothing_ForEmptyInput()
        Assert.Null(AcrDabTxtParser.Parse(""))
        Assert.Null(AcrDabTxtParser.Parse(Nothing))
    End Sub

End Class
