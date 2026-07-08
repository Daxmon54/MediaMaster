Imports System.Collections.Concurrent
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

Namespace Enrichment

    ''' <summary>
    ''' Resolves canonical artist/title spellings from the MusicBrainz web service
    ''' (free, no API key). Positive results are cached in-memory so a repeatedly-played track is
    ''' only looked up once; non-matches and failures are NOT cached, so a transient network error
    ''' doesn't permanently suppress a track. Any error or timeout yields a non-match, leaving the
    ''' original text unchanged.
    ''' </summary>
    Public Class MusicBrainzTrackNameResolver
        Implements ITrackNameResolver

        Private Const SearchUrl As String = "https://musicbrainz.org/ws/2/recording"
        ' MusicBrainz requires a descriptive User-Agent identifying the application + a contact URL.
        Private Const UserAgentValue As String = "MediaMaster/1.0 (https://github.com/Daxmon54/MediaMaster)"

        Private ReadOnly _httpClient As HttpClient
        Private ReadOnly _cache As New ConcurrentDictionary(Of String, ResolvedName)(StringComparer.OrdinalIgnoreCase)

        Public Sub New(httpClient As HttpClient)
            _httpClient = httpClient
        End Sub

        Public Async Function ResolveAsync(artist As String, title As String, cancellationToken As CancellationToken) As Task(Of ResolvedName) Implements ITrackNameResolver.ResolveAsync
            Dim unmatched As New ResolvedName With {.Artist = artist, .Title = title, .Matched = False}

            If String.IsNullOrWhiteSpace(artist) OrElse String.IsNullOrWhiteSpace(title) Then
                Return unmatched
            End If

            Dim key = $"{artist}|{title}"
            Dim cached As ResolvedName = Nothing
            If _cache.TryGetValue(key, cached) Then
                Return cached
            End If

            Try
                Dim query = $"artist:""{EscapeLucene(artist)}"" AND recording:""{EscapeLucene(title)}"""
                Dim url = $"{SearchUrl}?query={Uri.EscapeDataString(query)}&fmt=json&limit=5"

                Using request As New HttpRequestMessage(HttpMethod.Get, url)
                    request.Headers.TryAddWithoutValidation("User-Agent", UserAgentValue)
                    Using response = Await _httpClient.SendAsync(request, cancellationToken)
                        response.EnsureSuccessStatusCode()
                        Dim json = Await response.Content.ReadAsStringAsync(cancellationToken)
                        Dim match = MusicBrainzResponseParser.FindCanonical(json, artist, title)
                        If match IsNot Nothing Then
                            _cache.TryAdd(key, match)
                            Return match
                        End If
                    End Using
                End Using
            Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException AndAlso cancellationToken.IsCancellationRequested)
                ' network / parse / timeout -> fall through to unmatched (leave text unchanged)
            Catch ex As OperationCanceledException
                ' timed out or cancelled -> leave text unchanged
            End Try

            Return unmatched
        End Function

        Private Shared Function EscapeLucene(value As String) As String
            ' Neutralize the Lucene special characters that would break the quoted query term.
            Return value.Replace("\", " ").Replace("""", " ")
        End Function

    End Class

End Namespace
