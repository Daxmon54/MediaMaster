Imports System.Windows
Imports MediaMaster.App.ViewModels

Namespace Views

    Partial Public Class RegistrationWindow
        Inherits Window

        Private ReadOnly _viewModel As RegistrationViewModel

        Public Sub New(viewModel As RegistrationViewModel)
            InitializeComponent()

            _viewModel = viewModel
            DataContext = _viewModel

            AddHandler _viewModel.RequestInfoMessage, AddressOf OnRequestInfoMessage
        End Sub

        Private Sub OnRequestInfoMessage(sender As Object, message As String)
            MessageBox.Show(message, "MediaMaster", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

    End Class

End Namespace
