Imports Microsoft.Extensions.Configuration

Namespace Configuration

    Public Class AppSettingsProvider
        Implements IAppSettingsProvider

        Private ReadOnly _settings As AppSettings

        Public Sub New(configuration As IConfiguration)
            _settings = New AppSettings()
            configuration.Bind(_settings)
            Validate(_settings)
        End Sub

        Public Function GetSettings() As AppSettings Implements IAppSettingsProvider.GetSettings
            Return _settings
        End Function

        Private Shared Sub Validate(settings As AppSettings)
            If settings.SystemType < 1 OrElse settings.SystemType > 6 Then
                Throw New InvalidOperationException(
                    $"AppSettings.SystemType must be 1-6 (1=Carmen, 2=ACR Direct, 3=ACR FTP, 4=OTS, 5=Caliope, 6=PowerStudio). Got {settings.SystemType}.")
            End If
        End Sub

    End Class

End Namespace
