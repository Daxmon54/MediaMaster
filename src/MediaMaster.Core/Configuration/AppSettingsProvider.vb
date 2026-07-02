Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Microsoft.Extensions.Configuration

Namespace Configuration

    Public Class AppSettingsProvider
        Implements IAppSettingsProvider

        Private ReadOnly _settings As AppSettings
        Private ReadOnly _filePath As String

        Public Sub New(configuration As IConfiguration, Optional settingsFilePath As String = Nothing)
            _settings = New AppSettings()
            configuration.Bind(_settings)
            Validate(_settings)
            _filePath = If(settingsFilePath, Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
        End Sub

        Public Function GetSettings() As AppSettings Implements IAppSettingsProvider.GetSettings
            Return _settings
        End Function

        Public Sub Save() Implements IAppSettingsProvider.Save
            Dim options As New JsonSerializerOptions With {.WriteIndented = True}
            options.Converters.Add(New JsonStringEnumConverter())
            Dim json = JsonSerializer.Serialize(_settings, options)
            File.WriteAllText(_filePath, json)
        End Sub

        Private Shared Sub Validate(settings As AppSettings)
            If settings.SystemType < 1 OrElse settings.SystemType > 6 Then
                Throw New InvalidOperationException(
                    $"AppSettings.SystemType must be 1-6 (1=Carmen, 2=ACR Direct, 3=ACR FTP, 4=OTS, 5=Caliope, 6=PowerStudio). Got {settings.SystemType}.")
            End If
        End Sub

    End Class

End Namespace
