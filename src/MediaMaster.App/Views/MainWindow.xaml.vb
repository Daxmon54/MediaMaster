Imports System.ComponentModel
Imports System.Windows
Imports MediaMaster.App.ViewModels

Namespace Views

    Partial Public Class MainWindow
        Inherits Window

        Private ReadOnly _viewModel As MainViewModel
        Private _confirmedQuit As Boolean = False

        Public Sub New(viewModel As MainViewModel)
            InitializeComponent()

            _viewModel = viewModel
            DataContext = _viewModel

            AddHandler _viewModel.RestoreRequested, AddressOf OnRestoreRequested
            AddHandler _viewModel.QuitRequested, AddressOf OnQuitRequested
            AddHandler _viewModel.MinimizeToTrayRequested, AddressOf OnMinimizeToTrayRequested
            AddHandler _viewModel.LogEntries.CollectionChanged, AddressOf OnLogEntriesChanged
        End Sub

        Private Sub OnRestoreRequested(sender As Object, e As EventArgs)
            ShowInTaskbar = True
            Show()
            WindowState = WindowState.Normal
            Activate()
        End Sub

        Private Sub OnMinimizeToTrayRequested(sender As Object, e As EventArgs)
            Hide()
            ShowInTaskbar = False
        End Sub

        Private Sub OnQuitRequested(sender As Object, e As EventArgs)
            ConfirmAndShutdown()
        End Sub

        Private Sub OnTrayRestoreClick(sender As Object, e As RoutedEventArgs)
            OnRestoreRequested(sender, EventArgs.Empty)
        End Sub

        Private Sub OnTrayQuitClick(sender As Object, e As RoutedEventArgs)
            ConfirmAndShutdown()
        End Sub

        Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
            If _confirmedQuit Then
                Return
            End If
            e.Cancel = True
            ConfirmAndShutdown()
        End Sub

        Private Sub ConfirmAndShutdown()
            Dim result = MessageBox.Show(
                "Are you sure you want to quit the program?",
                "MediaMaster",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)

            If result = MessageBoxResult.Yes Then
                _confirmedQuit = True
                Application.Current.Shutdown()
            End If
        End Sub

        Private Sub OnLogEntriesChanged(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs)
            If LogListBox.Items.Count > 0 Then
                LogListBox.ScrollIntoView(LogListBox.Items(LogListBox.Items.Count - 1))
            End If
        End Sub

    End Class

End Namespace
