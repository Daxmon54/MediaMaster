Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Extensions.Hosting

Namespace Polling

    ''' <summary>
    ''' Clears the on-screen track-update log once per day at midnight, matching the original WinDev
    ''' Klok() behavior (ListDeleteAll(LIST_Log) when the clock reads 00:00:00). Rather than polling
    ''' the clock every second like the original, this simply waits until the next midnight, clears,
    ''' and repeats.
    ''' </summary>
    Public Class MidnightLogResetService
        Inherits BackgroundService

        Private ReadOnly _uiSink As IUiUpdateSink

        Public Sub New(uiSink As IUiUpdateSink)
            _uiSink = uiSink
        End Sub

        Protected Overrides Async Function ExecuteAsync(stoppingToken As CancellationToken) As Task
            Try
                While Not stoppingToken.IsCancellationRequested
                    Await Task.Delay(TimeUntilNextMidnight(), stoppingToken)
                    _uiSink.ClearLog()
                End While
            Catch ex As OperationCanceledException
                ' expected during shutdown
            End Try
        End Function

        Private Shared Function TimeUntilNextMidnight() As TimeSpan
            Dim now = DateTime.Now
            Dim nextMidnight = now.Date.AddDays(1)
            Dim delay = nextMidnight - now
            ' Guard against a zero/negative delay landing exactly on midnight (Task.Delay requires > 0).
            If delay <= TimeSpan.Zero Then
                delay = TimeSpan.FromSeconds(1)
            End If
            Return delay
        End Function

    End Class

End Namespace
