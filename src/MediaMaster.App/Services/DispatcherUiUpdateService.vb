Imports System.Windows.Threading
Imports MediaMaster.App.ViewModels
Imports MediaMaster.Core.Polling

Namespace Services

    ''' <summary>
    ''' The only class in the app that touches Dispatcher directly. Marshals Core's background-thread
    ''' status/log/progress updates onto the UI thread and applies them to MainViewModel.
    ''' </summary>
    Public Class DispatcherUiUpdateService
        Implements IUiUpdateSink

        Private ReadOnly _viewModel As MainViewModel
        Private ReadOnly _dispatcher As Dispatcher

        Public Sub New(viewModel As MainViewModel, dispatcher As Dispatcher)
            _viewModel = viewModel
            _dispatcher = dispatcher
        End Sub

        Public Sub UpdateStatus(statusText As String) Implements IUiUpdateSink.UpdateStatus
            _dispatcher.BeginInvoke(Sub() _viewModel.UpdateStatus(statusText))
        End Sub

        Public Sub AppendLog(logLine As String) Implements IUiUpdateSink.AppendLog
            _dispatcher.BeginInvoke(Sub() _viewModel.AppendLog(logLine))
        End Sub

        Public Sub ClearLog() Implements IUiUpdateSink.ClearLog
            _dispatcher.BeginInvoke(Sub() _viewModel.ClearLog())
        End Sub

        Public Sub SetProgress(current As Integer, maximum As Integer) Implements IUiUpdateSink.SetProgress
            _dispatcher.BeginInvoke(Sub() _viewModel.SetProgress(current, maximum))
        End Sub

    End Class

End Namespace
