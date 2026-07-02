Imports System.Windows
Imports MediaMaster.App.ViewModels

Namespace Views

    Partial Public Class SourceSettingsWindow
        Inherits Window

        Private ReadOnly _viewModel As SourceSettingsViewModel
        Private _suppressPasswordChanged As Boolean = False

        Public Sub New(viewModel As SourceSettingsViewModel)
            InitializeComponent()

            _viewModel = viewModel
            DataContext = _viewModel

            AddHandler _viewModel.RequestClose, AddressOf OnRequestClose
            AddHandler Loaded, AddressOf OnLoaded
        End Sub

        Private Sub OnLoaded(sender As Object, e As RoutedEventArgs)
            ' PasswordBox.Password isn't a bindable DependencyProperty (by design, to avoid it
            ' lingering in binding/undo infrastructure), so it's synced manually instead.
            _suppressPasswordChanged = True
            AcrFtpPasswordBox.Password = _viewModel.AcrFtpPassword
            _suppressPasswordChanged = False
        End Sub

        Private Sub OnAcrFtpPasswordChanged(sender As Object, e As RoutedEventArgs)
            If Not _suppressPasswordChanged Then
                _viewModel.AcrFtpPassword = AcrFtpPasswordBox.Password
            End If
        End Sub

        Private Sub OnRequestClose(sender As Object, saved As Boolean)
            DialogResult = saved
            Close()
        End Sub

        Private Sub OnBrowsePflFolder(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFolder(_viewModel.PflFolder)
            If path IsNot Nothing Then
                _viewModel.PflFolder = path
            End If
        End Sub

        Private Sub OnBrowseOtsFolder(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFolder(_viewModel.OtsWatchFolder)
            If path IsNot Nothing Then
                _viewModel.OtsWatchFolder = path
            End If
        End Sub

        Private Sub OnBrowseCaliopeFile(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFile(_viewModel.CaliopeWatchFile, "Text files (*.txt)|*.txt|All files (*.*)|*.*")
            If path IsNot Nothing Then
                _viewModel.CaliopeWatchFile = path
            End If
        End Sub

        Private Sub OnBrowsePowerStudioFile(sender As Object, e As RoutedEventArgs)
            Dim path = BrowseForFile(_viewModel.PowerStudioWatchFile, "XML files (*.xml)|*.xml|All files (*.*)|*.*")
            If path IsNot Nothing Then
                _viewModel.PowerStudioWatchFile = path
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

        Private Shared Function BrowseForFile(initialPath As String, filter As String) As String
            Dim dialog As New Microsoft.Win32.OpenFileDialog With {.Filter = filter}
            If Not String.IsNullOrEmpty(initialPath) AndAlso IO.File.Exists(initialPath) Then
                dialog.InitialDirectory = IO.Path.GetDirectoryName(initialPath)
                dialog.FileName = IO.Path.GetFileName(initialPath)
            End If
            If dialog.ShowDialog() = True Then
                Return dialog.FileName
            End If
            Return Nothing
        End Function

    End Class

End Namespace
