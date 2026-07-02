Namespace Configuration

    Public Enum DisplayOrderMode
        ArtistTitle = 1
        ArtistTitleYear = 2
        TitleArtist = 3
        TitleArtistYear = 4
    End Enum

    Public Class DisplayOrderSettings
        Public Property Mode As DisplayOrderMode = DisplayOrderMode.ArtistTitle
        Public Property Separator1 As String = "-"
        Public Property Separator2 As String = "-"
    End Class

    Public Class ExportSettings
        Public Property Path As String = ""
        Public Property ExtendedFileName As String = "ExtendedLivetrack.txt"
    End Class

    Public Class TracklogSettings
        ''' <summary>Insert a row into the MusicLog table (ACR Direct source only).</summary>
        Public Property LogToDatabase As Boolean
        ''' <summary>Append a line to a daily YYYY-MM-DD-playlog.txt file.</summary>
        Public Property LogToFile As Boolean
        Public Property FolderPath As String = ""
    End Class

    Public Class LiveTrackSettings
        Public Property Enabled As Boolean
        Public Property FolderPath As String = ""
    End Class

    Public Class DescriptionSettings
        Public Property DefaultText As String = ""
        Public Property TimeOfTheHour As String = ""
        Public Property Commercial As String = ""
        Public Property Info As String = ""
        ''' <summary>10 configurable descriptions for Carmen "event" slots.</summary>
        Public Property Events As String() = New String(9) {}
    End Class

    Public Class CarmenSettings
        Public Property Address As String = "127.0.0.1"
        Public Property LocalPort As Integer = 5900
        Public Property RemotePort As Integer = 5901
        ''' <summary>1 = all info (song + events), 2 = song info only.</summary>
        Public Property Mode As Integer = 1
        Public Property Extended As Boolean
    End Class

    Public Class AcrFtpSettings
        Public Property Host As String = ""
        Public Property UserName As String = ""
        Public Property Password As String = ""
        Public Property PollIntervalSeconds As Integer = 15
    End Class

    Public Class AcrDirectSettings
        Public Property BaseUrl As String = ""
        Public Property ProjectId As Integer
        Public Property ChannelId As Integer
        Public Property Token As String = ""
        Public Property PollIntervalSeconds As Integer = 20
    End Class

    Public Class OtsSettings
        Public Property WatchFolder As String = ""
    End Class

    Public Class CaliopeSettings
        Public Property WatchFile As String = ""
    End Class

    Public Class PowerStudioSettings
        Public Property WatchFile As String = ""
    End Class

    Public Class AppSettings
        ''' <summary>1=Carmen, 2=ACR Direct, 3=ACR FTP, 4=OTS, 5=Caliope, 6=PowerStudio.</summary>
        Public Property SystemType As Integer = 1
        Public Property Language As String = "nl"
        Public Property EditionId As Integer
        Public Property ReplaceAmpersand As Boolean
        Public Property TraceEnabled As Boolean
        ''' <summary>On-air/off-air semaphore folder; when a PFL_A.SEM/PFL_B.SEM file is present, publishing is suppressed.</summary>
        Public Property PflFolder As String = ""
        Public Property ConnectionString As String = "Data Source=mediamaster.db"

        Public Property DisplayOrder As New DisplayOrderSettings()
        Public Property Export As New ExportSettings()
        Public Property Tracklog As New TracklogSettings()
        Public Property LiveTrack As New LiveTrackSettings()
        Public Property Descriptions As New DescriptionSettings()
        Public Property Carmen As New CarmenSettings()
        Public Property AcrFtp As New AcrFtpSettings()
        Public Property AcrDirect As New AcrDirectSettings()
        Public Property Ots As New OtsSettings()
        Public Property Caliope As New CaliopeSettings()
        Public Property PowerStudio As New PowerStudioSettings()
    End Class

End Namespace
