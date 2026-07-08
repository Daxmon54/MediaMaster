Imports System.Text
Imports System.Text.Json

Namespace Enrichment

    ''' <summary>
    ''' Parses a MusicBrainz recording-search response and returns the canonical spelling of the
    ''' matching recording. Match is deliberately conservative: a recording only counts as "the same
    ''' track" when its artist and title, normalized to upper-case alphanumerics, equal the input's.
    ''' That way only the casing/punctuation of the SAME track is corrected -- a fuzzy MusicBrainz hit
    ''' for a different song is never substituted. Returns Nothing when there's no confident match.
    ''' </summary>
    Public NotInheritable Class MusicBrainzResponseParser

        Private Sub New()
        End Sub

        Public Shared Function FindCanonical(rawJson As String, inputArtist As String, inputTitle As String) As ResolvedName
            If String.IsNullOrWhiteSpace(rawJson) Then
                Return Nothing
            End If

            Try
                Using document = JsonDocument.Parse(rawJson)
                    Dim recordings As JsonElement
                    If Not document.RootElement.TryGetProperty("recordings", recordings) Then
                        Return Nothing
                    End If
                    If recordings.ValueKind <> JsonValueKind.Array Then
                        Return Nothing
                    End If

                    Dim normalizedInputArtist = Normalize(inputArtist)
                    Dim normalizedInputTitle = Normalize(inputTitle)

                    For Each recording In recordings.EnumerateArray()
                        Dim title = GetString(recording, "title")
                        Dim artist = BuildArtistCredit(recording)
                        If title Is Nothing OrElse artist Is Nothing Then
                            Continue For
                        End If

                        If Normalize(artist) = normalizedInputArtist AndAlso Normalize(title) = normalizedInputTitle Then
                            Return New ResolvedName With {.Artist = artist, .Title = title, .Matched = True}
                        End If
                    Next

                    Return Nothing
                End Using
            Catch ex As JsonException
                Return Nothing
            End Try
        End Function

        ''' <summary>Reconstructs the full credited artist string, honouring joinphrases (e.g. "A feat. B").</summary>
        Private Shared Function BuildArtistCredit(recording As JsonElement) As String
            Dim artistCredit As JsonElement
            If Not recording.TryGetProperty("artist-credit", artistCredit) OrElse artistCredit.ValueKind <> JsonValueKind.Array Then
                Return Nothing
            End If

            Dim builder As New StringBuilder()
            For Each entry In artistCredit.EnumerateArray()
                Dim name = GetString(entry, "name")
                If name IsNot Nothing Then
                    builder.Append(name)
                End If
                Dim joinPhrase = GetString(entry, "joinphrase")
                If joinPhrase IsNot Nothing Then
                    builder.Append(joinPhrase)
                End If
            Next

            Dim result = builder.ToString().Trim()
            Return If(result.Length > 0, result, Nothing)
        End Function

        Private Shared Function GetString(element As JsonElement, propertyName As String) As String
            Dim value As JsonElement
            If element.TryGetProperty(propertyName, value) AndAlso value.ValueKind = JsonValueKind.String Then
                Return value.GetString()
            End If
            Return Nothing
        End Function

        ''' <summary>Upper-cases and strips everything but letters/digits, so only real word differences (not case/punctuation) prevent a match.</summary>
        Public Shared Function Normalize(value As String) As String
            If value Is Nothing Then
                Return String.Empty
            End If

            Dim builder As New StringBuilder(value.Length)
            For Each c In value.ToUpperInvariant()
                If Char.IsLetterOrDigit(c) Then
                    builder.Append(c)
                End If
            Next
            Return builder.ToString()
        End Function

    End Class

End Namespace
