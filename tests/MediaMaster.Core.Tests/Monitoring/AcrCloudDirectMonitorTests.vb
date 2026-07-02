Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class FakeHttpMessageHandler
    Inherits HttpMessageHandler

    Public Property LastRequest As HttpRequestMessage
    Public Property ResponseJson As String = "{}"

    Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        LastRequest = request
        Dim response As New HttpResponseMessage(HttpStatusCode.OK) With {
            .Content = New StringContent(ResponseJson)
        }
        Return Task.FromResult(response)
    End Function
End Class

Public Class AcrCloudDirectMonitorTests

    Private Shared Function BuildJson(artist As String, title As String) As String
        Return $"{{ ""data"": {{ ""metadata"": {{ ""music"": [ {{ ""title"": ""{title}"", ""artists"": [ {{ ""name"": ""{artist}"" }} ] }} ] }} }} }}"
    End Function

    Private Shared Async Function WaitForTrackChangedAsync(monitor As AcrCloudDirectMonitor, Optional timeoutSeconds As Integer = 10) As Task(Of TrackInfo)
        Dim completionSource As New TaskCompletionSource(Of TrackInfo)()
        Dim handler As EventHandler(Of TrackInfo) = Sub(sender, track) completionSource.TrySetResult(track)
        AddHandler monitor.TrackChanged, handler
        Try
            Dim completed = Await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)))
            If completed IsNot completionSource.Task Then
                Return Nothing
            End If
            Return Await completionSource.Task
        Finally
            RemoveHandler monitor.TrackChanged, handler
        End Try
    End Function

    <Fact>
    Public Async Function RaisesTrackChanged_WithParsedTrack_AndBearerToken() As Task
        Dim fakeHandler As New FakeHttpMessageHandler With {.ResponseJson = BuildJson("ABBA", "Dancing Queen")}
        Dim httpClient As New HttpClient(fakeHandler)
        Dim settings As New AcrDirectSettings With {
            .BaseUrl = "https://api.example.test/projects/",
            .ProjectId = 42,
            .ChannelId = 7,
            .Token = "secret-token",
            .PollIntervalSeconds = 1
        }
        Dim monitor As New AcrCloudDirectMonitor(settings, httpClient)

        Await monitor.StartAsync(CancellationToken.None)
        Dim track = Await WaitForTrackChangedAsync(monitor)
        Await monitor.StopAsync()

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)

        Assert.NotNull(fakeHandler.LastRequest)
        Assert.Equal("Bearer", fakeHandler.LastRequest.Headers.Authorization.Scheme)
        Assert.Equal("secret-token", fakeHandler.LastRequest.Headers.Authorization.Parameter)
        Assert.Equal("https://api.example.test/projects/42/channels/7/realtime_results", fakeHandler.LastRequest.RequestUri.ToString())
    End Function

    <Fact>
    Public Async Function DoesNotRaiseTrackChanged_WhenTrackIsUnchanged() As Task
        Dim fakeHandler As New FakeHttpMessageHandler With {.ResponseJson = BuildJson("ABBA", "Dancing Queen")}
        Dim httpClient As New HttpClient(fakeHandler)
        Dim settings As New AcrDirectSettings With {
            .BaseUrl = "https://api.example.test/projects/",
            .ProjectId = 1,
            .ChannelId = 1,
            .Token = "t",
            .PollIntervalSeconds = 1
        }
        Dim monitor As New AcrCloudDirectMonitor(settings, httpClient)

        Await monitor.StartAsync(CancellationToken.None)
        Dim firstTrack = Await WaitForTrackChangedAsync(monitor)
        Assert.NotNull(firstTrack)

        Dim secondTrack = Await WaitForTrackChangedAsync(monitor, timeoutSeconds:=3)
        Await monitor.StopAsync()

        Assert.Null(secondTrack)
    End Function

End Class
