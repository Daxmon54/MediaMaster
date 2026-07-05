Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Threading
Imports MediaMaster.App.Services
Imports MediaMaster.App.ViewModels
Imports MediaMaster.Core.Configuration

Namespace Views

    Partial Public Class MainWindow
        Inherits Window

        Private ReadOnly _viewModel As MainViewModel
        Private ReadOnly _settingsProvider As IAppSettingsProvider
        Private ReadOnly _settingsWindowFactory As Func(Of SettingsWindow)
        Private ReadOnly _destinationSettingsWindowFactory As Func(Of DestinationSettingsWindow)
        Private ReadOnly _sourceSettingsWindowFactory As Func(Of SourceSettingsWindow)
        Private ReadOnly _registrationWindowFactory As Func(Of RegistrationWindow)
        Private _confirmedQuit As Boolean = False

        Public Sub New(viewModel As MainViewModel,
                        settingsProvider As IAppSettingsProvider,
                        settingsWindowFactory As Func(Of SettingsWindow),
                        destinationSettingsWindowFactory As Func(Of DestinationSettingsWindow),
                        sourceSettingsWindowFactory As Func(Of SourceSettingsWindow),
                        registrationWindowFactory As Func(Of RegistrationWindow))
            InitializeComponent()

            _viewModel = viewModel
            _settingsProvider = settingsProvider
            _settingsWindowFactory = settingsWindowFactory
            _destinationSettingsWindowFactory = destinationSettingsWindowFactory
            _sourceSettingsWindowFactory = sourceSettingsWindowFactory
            _registrationWindowFactory = registrationWindowFactory
            DataContext = _viewModel

            UpdateThemeMenuChecks(_settingsProvider.GetSettings().Theme)

            AddHandler _viewModel.RestoreRequested, AddressOf OnRestoreRequested
            AddHandler _viewModel.QuitRequested, AddressOf OnQuitRequested
            AddHandler _viewModel.MinimizeToTrayRequested, AddressOf OnMinimizeToTrayRequested
            AddHandler _viewModel.LogEntries.CollectionChanged, AddressOf OnLogEntriesChanged
        End Sub

        Private Sub OnThemeLightClick(sender As Object, e As RoutedEventArgs)
            ApplyTheme(AppTheme.Light)
        End Sub

        Private Sub OnThemeDarkClick(sender As Object, e As RoutedEventArgs)
            ApplyTheme(AppTheme.Dark)
        End Sub

        Private Sub ApplyTheme(theme As AppTheme)
            ThemeManager.Apply(theme)
            Dim settings = _settingsProvider.GetSettings()
            settings.Theme = theme
            _settingsProvider.Save()
            UpdateThemeMenuChecks(theme)
        End Sub

        Private Sub UpdateThemeMenuChecks(theme As AppTheme)
            ThemeLightMenuItem.IsChecked = (theme = AppTheme.Light)
            ThemeDarkMenuItem.IsChecked = (theme = AppTheme.Dark)
        End Sub

        Private Sub OnOpenRegistrationClick(sender As Object, e As RoutedEventArgs)
            Dim registrationWindow = _registrationWindowFactory()
            registrationWindow.Owner = Me
            registrationWindow.ShowDialog()
        End Sub

        Private Sub OnOpenSettingsClick(sender As Object, e As RoutedEventArgs)
            Dim settingsWindow = _settingsWindowFactory()
            settingsWindow.Owner = Me
            settingsWindow.ShowDialog()
        End Sub

        Private Sub OnOpenDestinationSettingsClick(sender As Object, e As RoutedEventArgs)
            Dim destinationWindow = _destinationSettingsWindowFactory()
            destinationWindow.Owner = Me
            destinationWindow.ShowDialog()
        End Sub

        Private Sub OnOpenSourceSettingsClick(sender As Object, e As RoutedEventArgs)
            Dim sourceWindow = _sourceSettingsWindowFactory()
            sourceWindow.Owner = Me
            Dim saved = sourceWindow.ShowDialog()
            If saved = True Then
                MessageBox.Show(
                    "Restart the program for the source changes to take effect.",
                    "MediaMaster",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information)
            End If
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

        Private Sub OnLogEntriesChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            If e.Action <> NotifyCollectionChangedAction.Add Then
                Return
            End If

            ' Defer to Background priority so this runs after the ListBox has processed the add and
            ' laid out. Select the newest line (mirrors the original WinDev ListSelectPlus) and scroll
            ' the inner ScrollViewer to the bottom -- ScrollIntoView on the very last item of a
            ' virtualizing ListBox is unreliable (it can land a row short because the extent isn't
            ' fully measured yet), whereas ScrollToBottom always pins the newest line into view.
            Dispatcher.BeginInvoke(
                Sub()
                    If LogListBox.Items.Count = 0 Then
                        Return
                    End If
                    LogListBox.SelectedItem = LogListBox.Items(LogListBox.Items.Count - 1)
                    Dim scrollViewer = FindDescendantScrollViewer(LogListBox)
                    If scrollViewer IsNot Nothing Then
                        scrollViewer.ScrollToBottom()
                    Else
                        LogListBox.ScrollIntoView(LogListBox.SelectedItem)
                    End If
                End Sub, DispatcherPriority.Background)
        End Sub

        Private Shared Function FindDescendantScrollViewer(root As DependencyObject) As ScrollViewer
            If TypeOf root Is ScrollViewer Then
                Return CType(root, ScrollViewer)
            End If
            For i = 0 To VisualTreeHelper.GetChildrenCount(root) - 1
                Dim result = FindDescendantScrollViewer(VisualTreeHelper.GetChild(root, i))
                If result IsNot Nothing Then
                    Return result
                End If
            Next
            Return Nothing
        End Function

    End Class

End Namespace
