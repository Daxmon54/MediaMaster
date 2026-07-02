Imports System.IO
Imports Microsoft.Extensions.Configuration
Imports MediaMaster.Core.Configuration
Imports Xunit

Public Class AppSettingsProviderTests

    Private Shared Function BuildConfiguration(systemType As Integer) As IConfiguration
        Dim data = New Dictionary(Of String, String) From {
            {"SystemType", systemType.ToString()}
        }
        Return New ConfigurationBuilder().AddInMemoryCollection(data).Build()
    End Function

    <Fact>
    Public Sub Save_WritesSettingsToDisk_AndTheyRoundTripCorrectly()
        Dim tempFile = Path.Combine(Path.GetTempPath(), $"mm-settings-{Guid.NewGuid()}.json")
        Try
            Dim provider As IAppSettingsProvider = New AppSettingsProvider(BuildConfiguration(5), tempFile)
            Dim settings = provider.GetSettings()
            settings.Language = "nl"
            settings.EditionId = 3
            settings.Descriptions.TimeOfTheHour = "Top of the hour"
            settings.DisplayOrder.Mode = DisplayOrderMode.TitleArtistYear

            provider.Save()

            Assert.True(File.Exists(tempFile))

            ' Reload from the saved file into a fresh provider, proving the JSON round-trips
            ' (including the enum, which is written as its string name via JsonStringEnumConverter).
            Dim reloadedConfig = New ConfigurationBuilder().AddJsonFile(tempFile).Build()
            Dim reloadedProvider As IAppSettingsProvider = New AppSettingsProvider(reloadedConfig, tempFile)
            Dim reloaded = reloadedProvider.GetSettings()

            Assert.Equal("nl", reloaded.Language)
            Assert.Equal(3, reloaded.EditionId)
            Assert.Equal("Top of the hour", reloaded.Descriptions.TimeOfTheHour)
            Assert.Equal(DisplayOrderMode.TitleArtistYear, reloaded.DisplayOrder.Mode)
        Finally
            If File.Exists(tempFile) Then
                File.Delete(tempFile)
            End If
        End Try
    End Sub

    <Fact>
    Public Sub Save_MutatingNestedObjectInPlace_KeepsTheSameReference()
        ' Monitors (e.g. CarmenUdpMonitor) hold a direct reference to settings.Descriptions rather than
        ' re-fetching it -- Save() must not replace that object, only mutate its properties, or a saved
        ' change from the Settings window wouldn't be visible to an already-running monitor.
        Dim tempFile = Path.Combine(Path.GetTempPath(), $"mm-settings-{Guid.NewGuid()}.json")
        Try
            Dim provider As IAppSettingsProvider = New AppSettingsProvider(BuildConfiguration(1), tempFile)
            Dim settings = provider.GetSettings()
            Dim descriptionsReference = settings.Descriptions

            settings.Descriptions.Commercial = "Ad break"
            provider.Save()

            Assert.Same(descriptionsReference, provider.GetSettings().Descriptions)
            Assert.Equal("Ad break", descriptionsReference.Commercial)
        Finally
            If File.Exists(tempFile) Then
                File.Delete(tempFile)
            End If
        End Try
    End Sub

End Class
