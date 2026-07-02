Imports System.Collections.Generic
Imports System.Text.Json

Namespace Monitoring

    ''' <summary>
    ''' Parses an ACRCloud "realtime_results" response into a TrackInfo, matching the original's
    ''' jz.data.metadata.music[1] (1-based; here music(0), 0-based) field reads: artist/title/label/
    ''' album name/first ISRC/year (first 4 characters of release_date).
    ''' </summary>
    Public NotInheritable Class AcrDirectResponseParser

        Private Sub New()
        End Sub

        Public Shared Function Parse(rawJson As String) As TrackInfo
            If String.IsNullOrWhiteSpace(rawJson) Then
                Return Nothing
            End If

            Try
                Using document = JsonDocument.Parse(rawJson)
                    Dim music = document.RootElement.GetProperty("data").GetProperty("metadata").GetProperty("music")(0)
                    Dim artist = music.GetProperty("artists")(0).GetProperty("name").GetString()
                    Dim title = music.GetProperty("title").GetString()

                    If artist & title = "00" Then
                        Return Nothing
                    End If

                    Dim label = TryGetString(music, "label")
                    Dim album = TryGetNestedString(music, "album", "name")
                    Dim isrc = TryGetFirstArrayString(music, "external_ids", "isrc")
                    Dim releaseDate = TryGetString(music, "release_date")
                    Dim year = If(releaseDate IsNot Nothing AndAlso releaseDate.Length >= 4, releaseDate.Substring(0, 4), String.Empty)

                    Return New TrackInfo With {
                        .Artist = artist,
                        .Title = title,
                        .Label = If(label, String.Empty),
                        .Album = If(album, String.Empty),
                        .Isrc = If(isrc, String.Empty),
                        .Year = year,
                        .InfoType = 1
                    }
                End Using
            Catch ex As JsonException
                Return Nothing
            Catch ex As KeyNotFoundException
                Return Nothing
            Catch ex As IndexOutOfRangeException
                Return Nothing
            End Try
        End Function

        Private Shared Function TryGetString(element As JsonElement, propertyName As String) As String
            Dim value As JsonElement
            If element.TryGetProperty(propertyName, value) Then
                Return value.GetString()
            End If
            Return Nothing
        End Function

        Private Shared Function TryGetNestedString(element As JsonElement, propertyName As String, nestedPropertyName As String) As String
            Dim nested As JsonElement
            If element.TryGetProperty(propertyName, nested) AndAlso nested.ValueKind = JsonValueKind.Object Then
                Dim value As JsonElement
                If nested.TryGetProperty(nestedPropertyName, value) Then
                    Return value.GetString()
                End If
            End If
            Return Nothing
        End Function

        Private Shared Function TryGetFirstArrayString(element As JsonElement, propertyName As String, arrayPropertyName As String) As String
            Dim container As JsonElement
            If element.TryGetProperty(propertyName, container) Then
                Dim arr As JsonElement
                If container.TryGetProperty(arrayPropertyName, arr) AndAlso arr.ValueKind = JsonValueKind.Array AndAlso arr.GetArrayLength() > 0 Then
                    Return arr(0).GetString()
                End If
            End If
            Return Nothing
        End Function

    End Class

End Namespace
