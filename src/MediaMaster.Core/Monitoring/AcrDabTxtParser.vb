Imports System.Text.Json

Namespace Monitoring

    ''' <summary>
    ''' Parses ACRCloud's "dab.txt" -- a single JSON object (not an array), wrapped in [] before
    ''' parsing to match the original's "[" &amp; sJson &amp; "]" trick -- and extracts the current
    ''' artist/title from data.metadata.music[0].artists[0] (0-based; the original WLanguage arrays
    ''' are 1-based: music[1].artists[1]).
    ''' </summary>
    Public NotInheritable Class AcrDabTxtParser

        Private Sub New()
        End Sub

        Public Shared Function Parse(rawJson As String) As TrackInfo
            If String.IsNullOrWhiteSpace(rawJson) Then
                Return Nothing
            End If

            Using document = JsonDocument.Parse("[" & rawJson & "]")
                Dim root = document.RootElement(0)
                Dim music = root.GetProperty("data").GetProperty("metadata").GetProperty("music")(0)
                Dim artist = music.GetProperty("artists")(0).GetProperty("name").GetString()
                Dim title = music.GetProperty("title").GetString()

                If artist = "0" OrElse title = "0" Then
                    Return Nothing
                End If

                Return New TrackInfo With {.Artist = artist, .Title = title, .Year = "0", .InfoType = 101}
            End Using
        End Function

    End Class

End Namespace
