Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Input
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports FluentFTP
Imports MediaMaster.Core.Configuration

Namespace ViewModels

    ''' <summary>
    ''' Ports WIN_Source: the "which now-playing source" selector and its 6 per-source settings
    ''' tabs (Carmen, ACR Direct, ACR FTP, OTS, Caliope, PowerStudio), all of which already map
    ''' onto the existing AppSettings shape.
    '''
    ''' Unlike SettingsViewModel/DestinationSettingsViewModel, changes here genuinely need a
    ''' restart to take effect: SourceMonitorFactory only runs once, at DI startup, to build the
    ''' single active ISourceMonitor -- so the original's "restart the program" message is kept
    ''' for this window specifically.
    '''
    ''' The original's window-opening code hardcoded the initially-shown tab to the first pane
    ''' ("WIN_Source..Plane = 1 // gnSystemType", with the trailing comment suggesting the intent
    ''' was `= gnSystemType` but it was left hardcoded) -- here the tab is correctly selected to
    ''' match the actual configured SystemType instead.
    ''' </summary>
    Public Class SourceSettingsViewModel
        Inherits ObservableObject

        Private ReadOnly _settingsProvider As IAppSettingsProvider

        Public Event RequestClose As EventHandler(Of Boolean)

        Public Sub New(settingsProvider As IAppSettingsProvider)
            _settingsProvider = settingsProvider

            SaveCommand = New RelayCommand(AddressOf OnSave)
            CancelCommand = New RelayCommand(AddressOf OnCancel)
            TestFtpConnectionCommand = New AsyncRelayCommand(AddressOf OnTestFtpConnectionAsync)

            LoadFromSettings()
        End Sub

        Public ReadOnly Property SaveCommand As ICommand
        Public ReadOnly Property CancelCommand As ICommand
        Public ReadOnly Property TestFtpConnectionCommand As ICommand

#Region "System type"

        Private _systemType As Integer = 1
        Public Property SystemType As Integer
            Get
                Return _systemType
            End Get
            Set(value As Integer)
                If SetProperty(_systemType, value) Then
                    OnPropertyChanged(NameOf(IsCarmenModeVisible))
                    OnPropertyChanged(NameOf(SelectedTabIndex))
                End If
            End Set
        End Property

        ''' <summary>Matches the original's COMBO_Source_Change: hidden once a non-Carmen-adjacent source (4-6) is picked.</summary>
        Public ReadOnly Property IsCarmenModeVisible As Boolean
            Get
                Return SystemType <= 3
            End Get
        End Property

        ''' <summary>0-based tab index matching SystemType (1-6); replaces the original's hardcoded-to-first-tab behavior.</summary>
        Public ReadOnly Property SelectedTabIndex As Integer
            Get
                Return Math.Max(SystemType - 1, 0)
            End Get
        End Property

        Private _carmenMode As Integer = 1
        Public Property CarmenMode As Integer
            Get
                Return _carmenMode
            End Get
            Set(value As Integer)
                SetProperty(_carmenMode, value)
            End Set
        End Property

#End Region

#Region "Carmen"

        Private _carmenAddress As String = String.Empty
        Public Property CarmenAddress As String
            Get
                Return _carmenAddress
            End Get
            Set(value As String)
                SetProperty(_carmenAddress, value)
            End Set
        End Property

        Private _carmenLocalPort As Integer
        Public Property CarmenLocalPort As Integer
            Get
                Return _carmenLocalPort
            End Get
            Set(value As Integer)
                SetProperty(_carmenLocalPort, value)
            End Set
        End Property

        Private _carmenRemotePort As Integer
        Public Property CarmenRemotePort As Integer
            Get
                Return _carmenRemotePort
            End Get
            Set(value As Integer)
                SetProperty(_carmenRemotePort, value)
            End Set
        End Property

        Private _carmenExtended As Boolean
        Public Property CarmenExtended As Boolean
            Get
                Return _carmenExtended
            End Get
            Set(value As Boolean)
                SetProperty(_carmenExtended, value)
            End Set
        End Property

        Private _pflFolder As String = String.Empty
        Public Property PflFolder As String
            Get
                Return _pflFolder
            End Get
            Set(value As String)
                SetProperty(_pflFolder, value)
            End Set
        End Property

#End Region

#Region "ACR Direct"

        Private _acrUrl As String = String.Empty
        Public Property AcrUrl As String
            Get
                Return _acrUrl
            End Get
            Set(value As String)
                SetProperty(_acrUrl, value)
            End Set
        End Property

        Private _acrProjectId As Integer
        Public Property AcrProjectId As Integer
            Get
                Return _acrProjectId
            End Get
            Set(value As Integer)
                SetProperty(_acrProjectId, value)
            End Set
        End Property

        Private _acrChannelId As Integer
        Public Property AcrChannelId As Integer
            Get
                Return _acrChannelId
            End Get
            Set(value As Integer)
                SetProperty(_acrChannelId, value)
            End Set
        End Property

        Private _acrToken As String = String.Empty
        Public Property AcrToken As String
            Get
                Return _acrToken
            End Get
            Set(value As String)
                SetProperty(_acrToken, value)
            End Set
        End Property

        Private _acrPollSeconds As Integer
        Public Property AcrPollSeconds As Integer
            Get
                Return _acrPollSeconds
            End Get
            Set(value As Integer)
                SetProperty(_acrPollSeconds, value)
            End Set
        End Property

#End Region

#Region "ACR FTP"

        Private _acrFtpHost As String = String.Empty
        Public Property AcrFtpHost As String
            Get
                Return _acrFtpHost
            End Get
            Set(value As String)
                SetProperty(_acrFtpHost, value)
            End Set
        End Property

        Private _acrFtpUser As String = String.Empty
        Public Property AcrFtpUser As String
            Get
                Return _acrFtpUser
            End Get
            Set(value As String)
                SetProperty(_acrFtpUser, value)
            End Set
        End Property

        Private _acrFtpPassword As String = String.Empty
        Public Property AcrFtpPassword As String
            Get
                Return _acrFtpPassword
            End Get
            Set(value As String)
                SetProperty(_acrFtpPassword, value)
            End Set
        End Property

        Private _acrFtpPollSeconds As Integer
        Public Property AcrFtpPollSeconds As Integer
            Get
                Return _acrFtpPollSeconds
            End Get
            Set(value As Integer)
                SetProperty(_acrFtpPollSeconds, value)
            End Set
        End Property

        Private _ftpTestResult As String = String.Empty
        Public Property FtpTestResult As String
            Get
                Return _ftpTestResult
            End Get
            Set(value As String)
                SetProperty(_ftpTestResult, value)
            End Set
        End Property

#End Region

#Region "OTS / Caliope / PowerStudio"

        Private _otsWatchFolder As String = String.Empty
        Public Property OtsWatchFolder As String
            Get
                Return _otsWatchFolder
            End Get
            Set(value As String)
                SetProperty(_otsWatchFolder, value)
            End Set
        End Property

        Private _caliopeWatchFile As String = String.Empty
        Public Property CaliopeWatchFile As String
            Get
                Return _caliopeWatchFile
            End Get
            Set(value As String)
                SetProperty(_caliopeWatchFile, value)
            End Set
        End Property

        Private _powerStudioWatchFile As String = String.Empty
        Public Property PowerStudioWatchFile As String
            Get
                Return _powerStudioWatchFile
            End Get
            Set(value As String)
                SetProperty(_powerStudioWatchFile, value)
            End Set
        End Property

#End Region

        Private Sub LoadFromSettings()
            Dim settings = _settingsProvider.GetSettings()

            SystemType = settings.SystemType
            CarmenMode = settings.Carmen.Mode
            CarmenAddress = settings.Carmen.Address
            CarmenLocalPort = settings.Carmen.LocalPort
            CarmenRemotePort = settings.Carmen.RemotePort
            CarmenExtended = settings.Carmen.Extended
            PflFolder = settings.PflFolder

            AcrUrl = settings.AcrDirect.BaseUrl
            AcrProjectId = settings.AcrDirect.ProjectId
            AcrChannelId = settings.AcrDirect.ChannelId
            AcrToken = settings.AcrDirect.Token
            AcrPollSeconds = settings.AcrDirect.PollIntervalSeconds

            AcrFtpHost = settings.AcrFtp.Host
            AcrFtpUser = settings.AcrFtp.UserName
            AcrFtpPassword = settings.AcrFtp.Password
            AcrFtpPollSeconds = settings.AcrFtp.PollIntervalSeconds

            OtsWatchFolder = settings.Ots.WatchFolder
            CaliopeWatchFile = settings.Caliope.WatchFile
            PowerStudioWatchFile = settings.PowerStudio.WatchFile
        End Sub

        Private Sub OnSave()
            Dim settings = _settingsProvider.GetSettings()

            settings.SystemType = SystemType
            settings.Carmen.Mode = CarmenMode
            settings.Carmen.Address = CarmenAddress
            settings.Carmen.LocalPort = CarmenLocalPort
            settings.Carmen.RemotePort = CarmenRemotePort
            settings.Carmen.Extended = CarmenExtended
            settings.PflFolder = PflFolder

            settings.AcrDirect.BaseUrl = AcrUrl
            settings.AcrDirect.ProjectId = AcrProjectId
            settings.AcrDirect.ChannelId = AcrChannelId
            settings.AcrDirect.Token = AcrToken
            settings.AcrDirect.PollIntervalSeconds = AcrPollSeconds

            settings.AcrFtp.Host = AcrFtpHost
            settings.AcrFtp.UserName = AcrFtpUser
            settings.AcrFtp.Password = AcrFtpPassword
            settings.AcrFtp.PollIntervalSeconds = AcrFtpPollSeconds

            settings.Ots.WatchFolder = OtsWatchFolder
            settings.Caliope.WatchFile = CaliopeWatchFile
            settings.PowerStudio.WatchFile = PowerStudioWatchFile

            _settingsProvider.Save()

            RaiseEvent RequestClose(Me, True)
        End Sub

        Private Sub OnCancel()
            RaiseEvent RequestClose(Me, False)
        End Sub

        Private Async Function OnTestFtpConnectionAsync() As Task
            FtpTestResult = "Testing..."
            Try
                Using client As New AsyncFtpClient(AcrFtpHost, AcrFtpUser, AcrFtpPassword, 21, Nothing, Nothing)
                    Await client.Connect(CancellationToken.None)
                    FtpTestResult = "Connection succeeded."
                End Using
            Catch ex As Exception
                FtpTestResult = $"Connection failed: {ex.Message}"
            End Try
        End Function

    End Class

End Namespace
