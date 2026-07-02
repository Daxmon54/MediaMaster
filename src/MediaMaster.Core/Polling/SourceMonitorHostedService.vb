Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Extensions.Hosting
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline

Namespace Polling

    ''' <summary>Starts the configured ISourceMonitor and routes its TrackChanged events into ITrackPublisher.</summary>
    Public Class SourceMonitorHostedService
        Inherits BackgroundService

        Private ReadOnly _monitor As ISourceMonitor
        Private ReadOnly _publisher As ITrackPublisher
        Private ReadOnly _settingsProvider As IAppSettingsProvider

        Public Sub New(monitor As ISourceMonitor, publisher As ITrackPublisher, settingsProvider As IAppSettingsProvider)
            _monitor = monitor
            _publisher = publisher
            _settingsProvider = settingsProvider
        End Sub

        Protected Overrides Async Function ExecuteAsync(stoppingToken As CancellationToken) As Task
            AddHandler _monitor.TrackChanged, AddressOf OnTrackChanged
            Await _monitor.StartAsync(stoppingToken)

            Try
                Await Task.Delay(Timeout.Infinite, stoppingToken)
            Catch ex As OperationCanceledException
                ' expected during shutdown
            End Try

            ' VB doesn't allow Await inside Finally, so cleanup happens here instead;
            ' the only way out of the Try above is normal cancellation, not an unrelated exception.
            RemoveHandler _monitor.TrackChanged, AddressOf OnTrackChanged
            Await _monitor.StopAsync()
        End Function

        Private Async Sub OnTrackChanged(sender As Object, track As TrackInfo)
            Dim settings = _settingsProvider.GetSettings()
            Dim sourceLabel = SourceLabels.ForSystemType(settings.SystemType)
            Dim logToDatabase = settings.SystemType = 2 ' ACR Direct only, matches the original
            Await _publisher.PublishAsync(sourceLabel, track, logToDatabase, CancellationToken.None)
        End Sub

    End Class

End Namespace
