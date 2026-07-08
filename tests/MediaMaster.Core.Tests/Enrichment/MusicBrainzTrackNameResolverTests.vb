Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Enrichment
Imports Xunit

Public Class CountingHttpMessageHandler
    Inherits HttpMessageHandler

    Public Property RequestCount As Integer
    Public Property LastRequestUri As Uri
    Public Property LastUserAgent As String
    Public Property ResponseJson As String = "{}"

    Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        RequestCount += 1
        LastRequestUri = request.RequestUri
        If request.Headers.Contains("User-Agent") Then
            LastUserAgent = String.Join(" ", request.Headers.GetValues("User-Agent"))
        End If
        Dim response As New HttpResponseMessage(HttpStatusCode.OK) With {
            .Content = New StringContent(ResponseJson)
        }
        Return Task.FromResult(response)
    End Function
End Class

Public Class MusicBrainzTrackNameResolverTests

    Private Shared Function MatchingJson() As String
        Return "
        {
          ""recordings"": [
            { ""title"": ""Dancing Queen"", ""artist-credit"": [ { ""name"": ""ABBA"" } ] }
          ]
        }"
    End Function

    <Fact>
    Public Async Function ResolveAsync_ReturnsCanonicalCasing_AndSendsUserAgent() As Task
        Dim handler As New CountingHttpMessageHandler With {.ResponseJson = MatchingJson()}
        Dim resolver As New MusicBrainzTrackNameResolver(New HttpClient(handler))

        Dim result = Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)

        Assert.True(result.Matched)
        Assert.Equal("ABBA", result.Artist)
        Assert.Equal("Dancing Queen", result.Title)
        Assert.Contains("MediaMaster", handler.LastUserAgent)
    End Function

    <Fact>
    Public Async Function ResolveAsync_CachesPositiveResults() As Task
        Dim handler As New CountingHttpMessageHandler With {.ResponseJson = MatchingJson()}
        Dim resolver As New MusicBrainzTrackNameResolver(New HttpClient(handler))

        Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)
        Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)

        Assert.Equal(1, handler.RequestCount)
    End Function

    <Fact>
    Public Async Function ResolveAsync_LeavesUnchanged_AndDoesNotCache_WhenNoMatch() As Task
        ' Different track returned -> not a confident match; must not cache so it can retry later.
        Dim handler As New CountingHttpMessageHandler With {
            .ResponseJson = "{ ""recordings"": [ { ""title"": ""Waterloo"", ""artist-credit"": [ { ""name"": ""ABBA"" } ] } ] }"
        }
        Dim resolver As New MusicBrainzTrackNameResolver(New HttpClient(handler))

        Dim first = Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)
        Dim second = Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)

        Assert.False(first.Matched)
        Assert.Equal("ABBA", first.Artist)
        Assert.Equal("DANCING QUEEN", first.Title)
        Assert.Equal(2, handler.RequestCount)
    End Function

    <Fact>
    Public Async Function ResolveAsync_ReturnsUnchanged_OnHttpError() As Task
        Dim faulting As New FaultingHttpMessageHandler()
        Dim resolver As New MusicBrainzTrackNameResolver(New HttpClient(faulting))

        Dim result = Await resolver.ResolveAsync("ABBA", "DANCING QUEEN", CancellationToken.None)

        Assert.False(result.Matched)
        Assert.Equal("ABBA", result.Artist)
        Assert.Equal("DANCING QUEEN", result.Title)
    End Function

    Private Class FaultingHttpMessageHandler
        Inherits HttpMessageHandler
        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            Throw New HttpRequestException("simulated network failure")
        End Function
    End Class

End Class
