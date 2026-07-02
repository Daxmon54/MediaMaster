Imports System.Windows.Input
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports MediaMaster.Core.Configuration

Namespace ViewModels

    ''' <summary>
    ''' Ports WIN_Dest (the export destination window) -- just the export folder and extended-info
    ''' filename, which map directly onto AppSettings.Export. CBOX_HyperFile (the original's toggle
    ''' for also writing a legacy HyperFile-format export) is dropped: HFSQL/HyperFile isn't part of
    ''' this port (SQLite instead), matching the same reasoning that dropped WIN_Common's DB-server tab.
    ''' </summary>
    Public Class DestinationSettingsViewModel
        Inherits ObservableObject

        Private ReadOnly _settingsProvider As IAppSettingsProvider

        Public Event RequestClose As EventHandler(Of Boolean)

        Public Sub New(settingsProvider As IAppSettingsProvider)
            _settingsProvider = settingsProvider

            SaveCommand = New RelayCommand(AddressOf OnSave)
            CancelCommand = New RelayCommand(AddressOf OnCancel)

            Dim settings = _settingsProvider.GetSettings()
            FolderPath = settings.Export.Path
            ExtendedFileName = settings.Export.ExtendedFileName
        End Sub

        Public ReadOnly Property SaveCommand As ICommand
        Public ReadOnly Property CancelCommand As ICommand

        Private _folderPath As String = String.Empty
        Public Property FolderPath As String
            Get
                Return _folderPath
            End Get
            Set(value As String)
                SetProperty(_folderPath, value)
            End Set
        End Property

        Private _extendedFileName As String = String.Empty
        Public Property ExtendedFileName As String
            Get
                Return _extendedFileName
            End Get
            Set(value As String)
                SetProperty(_extendedFileName, value)
            End Set
        End Property

        Private Sub OnSave()
            Dim settings = _settingsProvider.GetSettings()
            settings.Export.Path = FolderPath
            settings.Export.ExtendedFileName = ExtendedFileName

            _settingsProvider.Save()

            RaiseEvent RequestClose(Me, True)
        End Sub

        Private Sub OnCancel()
            RaiseEvent RequestClose(Me, False)
        End Sub

    End Class

End Namespace
