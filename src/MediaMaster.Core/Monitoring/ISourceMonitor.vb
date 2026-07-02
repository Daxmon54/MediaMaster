Imports System.Threading
Imports System.Threading.Tasks

Namespace Monitoring

    ''' <summary>Strategy contract implemented by each of the 6 "now playing" source integrations.</summary>
    Public Interface ISourceMonitor
        Inherits IAsyncDisposable

        Event TrackChanged As EventHandler(Of TrackInfo)

        ''' <summary>Configured polling interval in milliseconds; 0 for event-driven/file-watcher sources.</summary>
        ReadOnly Property PollingIntervalMs As Integer

        Function StartAsync(cancellationToken As CancellationToken) As Task
        Function StopAsync() As Task
    End Interface

End Namespace
