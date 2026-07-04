Imports System.Windows
Imports MediaMaster.App.ViewModels

Namespace Views

    Partial Public Class SettingsWindow
        Inherits Window

        Private ReadOnly _viewModel As SettingsViewModel

        Public Sub New(viewModel As SettingsViewModel)
            InitializeComponent()

            _viewModel = viewModel
            DataContext = _viewModel

            AddHandler _viewModel.RequestClose, AddressOf OnRequestClose
        End Sub

        Private Sub OnRequestClose(sender As Object, saved As Boolean)
            DialogResult = saved
            Close()
        End Sub

        Private Sub OnBrowseTrackLogFolder(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFolder(_viewModel.TrackLogFolderPath)
            If path IsNot Nothing Then
                _viewModel.TrackLogFolderPath = path
            End If
        End Sub

        Private Sub OnBrowseLiveTrackFolder(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFolder(_viewModel.LiveTrackFolderPath)
            If path IsNot Nothing Then
                _viewModel.LiveTrackFolderPath = path
            End If
        End Sub

        Private Shared Function BrowseForFolder(initialPath As String) As String
            Using dialog As New System.Windows.Forms.FolderBrowserDialog()
                If Not String.IsNullOrEmpty(initialPath) AndAlso IO.Directory.Exists(initialPath) Then
                    dialog.SelectedPath = initialPath
                End If
                If dialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                    Return dialog.SelectedPath
                End If
            End Using
            Return Nothing
        End Function

    End Class

End Namespace
