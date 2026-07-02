Imports System.Windows
Imports MediaMaster.App.ViewModels

Namespace Views

    Partial Public Class DestinationSettingsWindow
        Inherits Window

        Private ReadOnly _viewModel As DestinationSettingsViewModel

        Public Sub New(viewModel As DestinationSettingsViewModel)
            InitializeComponent()

            _viewModel = viewModel
            DataContext = _viewModel

            AddHandler _viewModel.RequestClose, AddressOf OnRequestClose
        End Sub

        Private Sub OnRequestClose(sender As Object, saved As Boolean)
            DialogResult = saved
            Close()
        End Sub

        Private Sub OnBrowseFolder(sender As Object, e As RoutedEventArgs)
            Using dialog As New System.Windows.Forms.FolderBrowserDialog()
                If Not String.IsNullOrEmpty(_viewModel.FolderPath) AndAlso IO.Directory.Exists(_viewModel.FolderPath) Then
                    dialog.SelectedPath = _viewModel.FolderPath
                End If
                If dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                    _viewModel.FolderPath = dialog.SelectedPath
                End If
            End Using
        End Sub

    End Class

End Namespace
