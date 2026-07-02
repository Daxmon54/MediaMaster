Imports System.Net.Http
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports Moq
Imports Xunit

Public Class SourceMonitorFactoryTests

    Private Shared Function MakeFactory(systemType As Integer) As SourceMonitorFactory
        Dim settings As New AppSettings With {
            .SystemType = systemType,
            .Carmen = New CarmenSettings With {.LocalPort = 5900},
            .AcrDirect = New AcrDirectSettings With {.BaseUrl = "https://example.test/"},
            .AcrFtp = New AcrFtpSettings With {.Host = "ftp.example.test"},
            .Ots = New OtsSettings With {.WatchFolder = "C:\ots"},
            .Caliope = New CaliopeSettings With {.WatchFile = "C:\caliope\now.txt"},
            .PowerStudio = New PowerStudioSettings With {.WatchFolder = "C:\powerstudio"}
        }
        Dim settingsProvider = New Mock(Of IAppSettingsProvider)()
        settingsProvider.Setup(Function(s) s.GetSettings()).Returns(settings)

        Return New SourceMonitorFactory(settingsProvider.Object, New HttpClient())
    End Function

    <Theory>
    <InlineData(1, GetType(CarmenUdpMonitor))>
    <InlineData(2, GetType(AcrCloudDirectMonitor))>
    <InlineData(3, GetType(AcrCloudFtpMonitor))>
    <InlineData(4, GetType(OtsFileMonitor))>
    <InlineData(5, GetType(CaliopeFileMonitor))>
    <InlineData(6, GetType(PowerStudioFileMonitor))>
    Public Sub Create_ReturnsTheExpectedMonitorType_ForEachSystemType(systemType As Integer, expectedType As Type)
        Dim factory = MakeFactory(systemType)

        Dim monitor = factory.Create()

        Assert.IsType(expectedType, monitor)
    End Sub

    <Fact>
    Public Sub Create_Throws_ForUnsupportedSystemType()
        Dim factory = MakeFactory(99)

        Assert.Throws(Of InvalidOperationException)(Function() factory.Create())
    End Sub

End Class
