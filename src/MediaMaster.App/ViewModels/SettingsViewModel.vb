Imports System.Windows.Input
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input
Imports MediaMaster.Core.Configuration

Namespace ViewModels

    ''' <summary>
    ''' Ports the in-scope fields of the original WIN_Common settings window: general/tracklog/
    ''' livetrack settings and the 10 Carmen event descriptions. The DB-server tab (moot -- this
    ''' port uses local SQLite, not WinDev's HFSQL Client/Server) and the Filters CRUD tab (the
    ''' original FilterCheck logic was dead code, so there's nothing to configure) are intentionally
    ''' not ported.
    '''
    ''' Edits a local snapshot of the settings, not the live AppSettings object directly, so Cancel
    ''' can discard changes cleanly; Save copies the snapshot back into the live object *in place*
    ''' (mutating existing nested objects rather than replacing them) so already-running monitors
    ''' pick up the change immediately -- no restart needed, unlike the original.
    ''' </summary>
    Public Class SettingsViewModel
        Inherits ObservableObject

        Private ReadOnly _settingsProvider As IAppSettingsProvider

        Public Event RequestClose As EventHandler(Of Boolean)

        Public Sub New(settingsProvider As IAppSettingsProvider)
            _settingsProvider = settingsProvider

            SaveCommand = New RelayCommand(AddressOf OnSave)
            CancelCommand = New RelayCommand(AddressOf OnCancel)

            LoadFromSettings()
        End Sub

        Public ReadOnly Property SaveCommand As ICommand
        Public ReadOnly Property CancelCommand As ICommand

#Region "General"

        Private _traceEnabled As Boolean
        Public Property TraceEnabled As Boolean
            Get
                Return _traceEnabled
            End Get
            Set(value As Boolean)
                SetProperty(_traceEnabled, value)
            End Set
        End Property

        Private _logToDatabase As Boolean
        Public Property LogToDatabase As Boolean
            Get
                Return _logToDatabase
            End Get
            Set(value As Boolean)
                SetProperty(_logToDatabase, value)
            End Set
        End Property

        Private _logToFile As Boolean
        Public Property LogToFile As Boolean
            Get
                Return _logToFile
            End Get
            Set(value As Boolean)
                SetProperty(_logToFile, value)
            End Set
        End Property

        Private _trackLogFolderPath As String = String.Empty
        Public Property TrackLogFolderPath As String
            Get
                Return _trackLogFolderPath
            End Get
            Set(value As String)
                SetProperty(_trackLogFolderPath, value)
            End Set
        End Property

        Private _liveTrackEnabled As Boolean
        Public Property LiveTrackEnabled As Boolean
            Get
                Return _liveTrackEnabled
            End Get
            Set(value As Boolean)
                SetProperty(_liveTrackEnabled, value)
            End Set
        End Property

        Private _liveTrackFolderPath As String = String.Empty
        Public Property LiveTrackFolderPath As String
            Get
                Return _liveTrackFolderPath
            End Get
            Set(value As String)
                SetProperty(_liveTrackFolderPath, value)
            End Set
        End Property

#End Region

#Region "Descriptions"

        Private _defaultText As String = String.Empty
        Public Property DefaultText As String
            Get
                Return _defaultText
            End Get
            Set(value As String)
                SetProperty(_defaultText, value)
            End Set
        End Property

        Private _timeOfTheHour As String = String.Empty
        Public Property TimeOfTheHour As String
            Get
                Return _timeOfTheHour
            End Get
            Set(value As String)
                SetProperty(_timeOfTheHour, value)
            End Set
        End Property

        Private _commercial As String = String.Empty
        Public Property Commercial As String
            Get
                Return _commercial
            End Get
            Set(value As String)
                SetProperty(_commercial, value)
            End Set
        End Property

        Private _info As String = String.Empty
        Public Property Info As String
            Get
                Return _info
            End Get
            Set(value As String)
                SetProperty(_info, value)
            End Set
        End Property

        Private _event1 As String = String.Empty
        Public Property Event1 As String
            Get
                Return _event1
            End Get
            Set(value As String)
                SetProperty(_event1, value)
            End Set
        End Property

        Private _event2 As String = String.Empty
        Public Property Event2 As String
            Get
                Return _event2
            End Get
            Set(value As String)
                SetProperty(_event2, value)
            End Set
        End Property

        Private _event3 As String = String.Empty
        Public Property Event3 As String
            Get
                Return _event3
            End Get
            Set(value As String)
                SetProperty(_event3, value)
            End Set
        End Property

        Private _event4 As String = String.Empty
        Public Property Event4 As String
            Get
                Return _event4
            End Get
            Set(value As String)
                SetProperty(_event4, value)
            End Set
        End Property

        Private _event5 As String = String.Empty
        Public Property Event5 As String
            Get
                Return _event5
            End Get
            Set(value As String)
                SetProperty(_event5, value)
            End Set
        End Property

        Private _event6 As String = String.Empty
        Public Property Event6 As String
            Get
                Return _event6
            End Get
            Set(value As String)
                SetProperty(_event6, value)
            End Set
        End Property

        Private _event7 As String = String.Empty
        Public Property Event7 As String
            Get
                Return _event7
            End Get
            Set(value As String)
                SetProperty(_event7, value)
            End Set
        End Property

        Private _event8 As String = String.Empty
        Public Property Event8 As String
            Get
                Return _event8
            End Get
            Set(value As String)
                SetProperty(_event8, value)
            End Set
        End Property

        Private _event9 As String = String.Empty
        Public Property Event9 As String
            Get
                Return _event9
            End Get
            Set(value As String)
                SetProperty(_event9, value)
            End Set
        End Property

        Private _event10 As String = String.Empty
        Public Property Event10 As String
            Get
                Return _event10
            End Get
            Set(value As String)
                SetProperty(_event10, value)
            End Set
        End Property

#End Region

        Private Sub LoadFromSettings()
            Dim settings = _settingsProvider.GetSettings()

            TraceEnabled = settings.TraceEnabled
            LogToDatabase = settings.Tracklog.LogToDatabase
            LogToFile = settings.Tracklog.LogToFile
            TrackLogFolderPath = settings.Tracklog.FolderPath
            LiveTrackEnabled = settings.LiveTrack.Enabled
            LiveTrackFolderPath = settings.LiveTrack.FolderPath

            DefaultText = settings.Descriptions.DefaultText
            TimeOfTheHour = settings.Descriptions.TimeOfTheHour
            Commercial = settings.Descriptions.Commercial
            Info = settings.Descriptions.Info

            Dim events = settings.Descriptions.Events
            Event1 = EventAt(events, 0)
            Event2 = EventAt(events, 1)
            Event3 = EventAt(events, 2)
            Event4 = EventAt(events, 3)
            Event5 = EventAt(events, 4)
            Event6 = EventAt(events, 5)
            Event7 = EventAt(events, 6)
            Event8 = EventAt(events, 7)
            Event9 = EventAt(events, 8)
            Event10 = EventAt(events, 9)
        End Sub

        Private Shared Function EventAt(events As String(), index As Integer) As String
            If events IsNot Nothing AndAlso index < events.Length Then
                Return If(events(index), String.Empty)
            End If
            Return String.Empty
        End Function

        Private Sub OnSave()
            Dim settings = _settingsProvider.GetSettings()

            settings.TraceEnabled = TraceEnabled
            settings.Tracklog.LogToDatabase = LogToDatabase
            settings.Tracklog.LogToFile = LogToFile
            settings.Tracklog.FolderPath = TrackLogFolderPath
            settings.LiveTrack.Enabled = LiveTrackEnabled
            settings.LiveTrack.FolderPath = LiveTrackFolderPath

            settings.Descriptions.DefaultText = DefaultText
            settings.Descriptions.TimeOfTheHour = TimeOfTheHour
            settings.Descriptions.Commercial = Commercial
            settings.Descriptions.Info = Info

            If settings.Descriptions.Events Is Nothing OrElse settings.Descriptions.Events.Length < 10 Then
                settings.Descriptions.Events = New String(9) {}
            End If
            settings.Descriptions.Events(0) = Event1
            settings.Descriptions.Events(1) = Event2
            settings.Descriptions.Events(2) = Event3
            settings.Descriptions.Events(3) = Event4
            settings.Descriptions.Events(4) = Event5
            settings.Descriptions.Events(5) = Event6
            settings.Descriptions.Events(6) = Event7
            settings.Descriptions.Events(7) = Event8
            settings.Descriptions.Events(8) = Event9
            settings.Descriptions.Events(9) = Event10

            _settingsProvider.Save()

            RaiseEvent RequestClose(Me, True)
        End Sub

        Private Sub OnCancel()
            RaiseEvent RequestClose(Me, False)
        End Sub

    End Class

End Namespace
