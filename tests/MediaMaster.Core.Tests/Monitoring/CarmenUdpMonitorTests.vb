Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports Xunit

Public Class CarmenUdpMonitorTests

    Private Shared Function FreeUdpPort() As Integer
        Using probe As New UdpClient(0)
            Return CType(probe.Client.LocalEndPoint, IPEndPoint).Port
        End Using
    End Function

    Private Shared Async Function WaitForTrackChangedAsync(monitor As CarmenUdpMonitor, act As Func(Of Task), Optional timeoutSeconds As Integer = 10) As Task(Of TrackInfo)
        Dim completionSource As New TaskCompletionSource(Of TrackInfo)()
        Dim handler As EventHandler(Of TrackInfo) = Sub(sender, track) completionSource.TrySetResult(track)
        AddHandler monitor.TrackChanged, handler
        Try
            Await act()
            Dim completed = Await Task.WhenAny(completionSource.Task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)))
            If completed IsNot completionSource.Task Then
                Return Nothing
            End If
            Return Await completionSource.Task
        Finally
            RemoveHandler monitor.TrackChanged, handler
        End Try
    End Function

    Private Shared Async Function SendAsync(port As Integer, message As String) As Task
        Using sender As New UdpClient()
            Dim bytes = Encoding.UTF8.GetBytes(message)
            Await sender.SendAsync(bytes, bytes.Length, "127.0.0.1", port)
        End Using
    End Function

    <Fact>
    Public Async Function RaisesTrackChanged_ForSongMessage() As Task
        Dim port = FreeUdpPort()
        Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 1}
        Dim monitor As New CarmenUdpMonitor(settings, New DescriptionSettings(), pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, "Dancing Queen;ABBA;1976;1"))

        Assert.NotNull(track)
        Assert.Equal("ABBA", track.Artist)
        Assert.Equal("Dancing Queen", track.Title)
        Assert.Equal("1976", track.Year)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function SkipsAudioItemPlaceholder_InAllInfoMode() As Task
        Dim port = FreeUdpPort()
        Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 1}
        Dim monitor As New CarmenUdpMonitor(settings, New DescriptionSettings(), pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, "Some Title;Audio Item 42;0;1"), timeoutSeconds:=2)

        Assert.Null(track)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function ResolvesTimeSignalEvent_ToConfiguredDescription_InAllInfoMode() As Task
        Dim port = FreeUdpPort()
        Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 1}
        Dim descriptions As New DescriptionSettings With {.TimeOfTheHour = "Top of the hour"}
        Dim monitor As New CarmenUdpMonitor(settings, descriptions, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, ";;;5"))

        Assert.NotNull(track)
        Assert.Equal("Top of the hour", track.Artist)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function ResolvesEventSlot_ToConfiguredDescription_InAllInfoMode() As Task
        Dim port = FreeUdpPort()
        Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 1}
        Dim events(9) As String
        events(2) = "Traffic Update" ' eventType 32 -> index 2
        Dim descriptions As New DescriptionSettings With {.Events = events}
        Dim monitor As New CarmenUdpMonitor(settings, descriptions, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, ";;;32"))

        Assert.NotNull(track)
        Assert.Equal("Traffic Update", track.Artist)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function IgnoresNonSongEvents_InSongOnlyMode() As Task
        Dim port = FreeUdpPort()
        Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 2}
        Dim descriptions As New DescriptionSettings With {.TimeOfTheHour = "Top of the hour"}
        Dim monitor As New CarmenUdpMonitor(settings, descriptions, pflFolder:="")
        Await monitor.StartAsync(CancellationToken.None)

        Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, ";;;5"), timeoutSeconds:=2)

        Assert.Null(track)

        Await monitor.StopAsync()
    End Function

    <Fact>
    Public Async Function SuppressesPublishing_WhenPflSemaphorePresent() As Task
        Dim pflFolder = Path.Combine(Path.GetTempPath(), $"mm-carmen-pfl-{Guid.NewGuid()}")
        Directory.CreateDirectory(pflFolder)
        File.WriteAllText(Path.Combine(pflFolder, "PFL_B.SEM"), "")

        Try
            Dim port = FreeUdpPort()
            Dim settings As New CarmenSettings With {.LocalPort = port, .Mode = 1}
            Dim monitor As New CarmenUdpMonitor(settings, New DescriptionSettings(), pflFolder)
            Await monitor.StartAsync(CancellationToken.None)

            Dim track = Await WaitForTrackChangedAsync(monitor, Function() SendAsync(port, "Dancing Queen;ABBA;1976;1"), timeoutSeconds:=2)

            Assert.Null(track)

            Await monitor.StopAsync()
        Finally
            Directory.Delete(pflFolder, recursive:=True)
        End Try
    End Function

End Class
