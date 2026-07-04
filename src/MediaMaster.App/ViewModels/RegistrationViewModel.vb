Imports System.Windows.Input
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports MediaMaster.Core.Configuration

Namespace ViewModels

    ''' <summary>
    ''' Ports WIN_Registration: Name / Serial / Key fields and Generate / Unlock buttons.
    '''
    ''' The original's KeyGenerateInitialKey (derives a serial from a name) and KeyCompareKey
    ''' (validates a serial+key pair) are opaque WinDev built-ins -- their algorithms aren't in
    ''' the source anywhere, so neither can be faithfully reproduced. Rather than fabricate a
    ''' new scheme or silently fake success, both actions here just say so plainly. This is
    ''' consistent with the rest of the port: there's no license gate anywhere in this app: it
    ''' always runs unlocked.
    ''' </summary>
    Public Class RegistrationViewModel
        Inherits ObservableObject

        Private ReadOnly _settingsProvider As IAppSettingsProvider

        Public Event RequestInfoMessage As EventHandler(Of String)

        Public Sub New(settingsProvider As IAppSettingsProvider)
            _settingsProvider = settingsProvider

            GenerateSerialCommand = New RelayCommand(AddressOf OnGenerateSerial)
            UnlockCommand = New RelayCommand(AddressOf OnUnlock)

            Dim settings = _settingsProvider.GetSettings()
            Name = settings.RegisteredName
            SerialNumber = settings.SerialNumber
            LicenseKey = settings.LicenseKey
        End Sub

        Public ReadOnly Property GenerateSerialCommand As ICommand
        Public ReadOnly Property UnlockCommand As ICommand

        Private _name As String = String.Empty
        Public Property Name As String
            Get
                Return _name
            End Get
            Set(value As String)
                SetProperty(_name, value)
            End Set
        End Property

        Private _serialNumber As String = String.Empty
        Public Property SerialNumber As String
            Get
                Return _serialNumber
            End Get
            Set(value As String)
                SetProperty(_serialNumber, value)
            End Set
        End Property

        Private _licenseKey As String = String.Empty
        Public Property LicenseKey As String
            Get
                Return _licenseKey
            End Get
            Set(value As String)
                SetProperty(_licenseKey, value)
            End Set
        End Property

        Private Sub OnGenerateSerial()
            Dim settings = _settingsProvider.GetSettings()
            settings.RegisteredName = Name
            _settingsProvider.Save()

            RaiseEvent RequestInfoMessage(Me,
                "Serial generation isn't implemented in this build: the original KeyGenerateInitialKey " &
                "algorithm is a proprietary WinDev built-in with no available source, so it can't be " &
                "reproduced here. MediaMaster runs unlocked regardless.")
        End Sub

        Private Sub OnUnlock()
            Dim settings = _settingsProvider.GetSettings()
            settings.LicenseKey = LicenseKey
            _settingsProvider.Save()

            RaiseEvent RequestInfoMessage(Me,
                "License validation isn't implemented in this build: the original KeyCompareKey " &
                "algorithm is a proprietary WinDev built-in with no available source, so it can't be " &
                "reproduced here. MediaMaster runs unlocked regardless of what's entered above.")
        End Sub

    End Class

End Namespace
