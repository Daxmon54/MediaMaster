Imports System.Net.Http
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>Maps AppSettings.SystemType (1-6) to the concrete ISourceMonitor implementation, matching the original window-opening switch in WIN_Main.</summary>
    Public Class SourceMonitorFactory
        Implements ISourceMonitorFactory

        Private ReadOnly _settingsProvider As IAppSettingsProvider
        Private ReadOnly _httpClient As HttpClient

        Public Sub New(settingsProvider As IAppSettingsProvider, httpClient As HttpClient)
            _settingsProvider = settingsProvider
            _httpClient = httpClient
        End Sub

        Public Function Create() As ISourceMonitor Implements ISourceMonitorFactory.Create
            Dim settings = _settingsProvider.GetSettings()

            Select Case settings.SystemType
                Case 1
                    Return New CarmenUdpMonitor(settings.Carmen, settings.Descriptions, settings.PflFolder)
                Case 2
                    Return New AcrCloudDirectMonitor(settings.AcrDirect, _httpClient)
                Case 3
                    Return New AcrCloudFtpMonitor(settings.AcrFtp)
                Case 4
                    Return New OtsFileMonitor(settings.Ots, settings.PflFolder)
                Case 5
                    Return New CaliopeFileMonitor(settings.Caliope)
                Case 6
                    Return New PowerStudioFileMonitor(settings.PowerStudio)
                Case Else
                    Throw New InvalidOperationException($"Unsupported AppSettings.SystemType: {settings.SystemType} (expected 1-6).")
            End Select
        End Function

    End Class

End Namespace
