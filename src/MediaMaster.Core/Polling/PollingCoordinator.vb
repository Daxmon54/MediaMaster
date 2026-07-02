Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Extensions.Hosting
Imports MediaMaster.Core.Monitoring

Namespace Polling

    ''' <summary>
    ''' The "Klok" (1s timer) replacement, scoped to just its progress-bar-countdown responsibility.
    ''' For sources with a real polling interval (ACR FTP/Direct) this counts up to that interval;
    ''' for event-driven sources (file watchers, Carmen UDP) it just shows a small heartbeat, since
    ''' there's no "time to next poll" for those.
    ''' </summary>
    Public Class PollingCoordinator
        Inherits BackgroundService

        Private ReadOnly _uiSink As IUiUpdateSink
        Private ReadOnly _monitor As ISourceMonitor
        Private _elapsedTicks As Integer = 0

        Public Sub New(uiSink As IUiUpdateSink, monitor As ISourceMonitor)
            _uiSink = uiSink
            _monitor = monitor
        End Sub

        Protected Overrides Async Function ExecuteAsync(stoppingToken As CancellationToken) As Task
            Try
                Using timer As New PeriodicTimer(TimeSpan.FromSeconds(1))
                    While Await timer.WaitForNextTickAsync(stoppingToken)
                        Tick()
                    End While
                End Using
            Catch ex As OperationCanceledException
                ' expected during shutdown
            End Try
        End Function

        Private Sub Tick()
            If _monitor.PollingIntervalMs > 0 Then
                Dim maxSeconds = Math.Max(_monitor.PollingIntervalMs \ 1000, 1)
                _elapsedTicks = (_elapsedTicks + 1) Mod maxSeconds
                _uiSink.SetProgress(_elapsedTicks, maxSeconds)
            Else
                _elapsedTicks = (_elapsedTicks + 1) Mod 10
                _uiSink.SetProgress(_elapsedTicks, 10)
            End If
        End Sub

    End Class

End Namespace
