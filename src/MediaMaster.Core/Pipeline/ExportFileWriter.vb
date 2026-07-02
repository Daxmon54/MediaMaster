Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    Public Interface IExportFileWriter
        Sub Write(track As TrackInfo, exportSettings As ExportSettings)
    End Interface

    Public Class ExportPayload
        Public Property Artist As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Year As String = String.Empty
        Public Property Label As String = String.Empty
        Public Property Album As String = String.Empty
        <JsonPropertyName("ISRC")>
        Public Property Isrc As String = String.Empty
        Public Property InfoType As Integer
        Public Property Duration As String = String.Empty
    End Class

    ''' <summary>The CreateFile() export-file half: overwrites the configured JSON export file.</summary>
    Public Class ExportFileWriter
        Implements IExportFileWriter

        Public Sub Write(track As TrackInfo, exportSettings As ExportSettings) Implements IExportFileWriter.Write
            If String.IsNullOrEmpty(exportSettings.Path) Then
                Return
            End If

            If Not Directory.Exists(exportSettings.Path) Then
                Directory.CreateDirectory(exportSettings.Path)
            End If

            Dim payload As New ExportPayload With {
                .Artist = track.Artist,
                .Title = track.Title,
                .Year = track.Year,
                .Label = track.Label,
                .Album = track.Album,
                .Isrc = track.Isrc,
                .InfoType = track.InfoType,
                .Duration = track.Duration
            }

            Dim json = JsonSerializer.Serialize(payload)
            Dim fullPath = Path.Combine(exportSettings.Path, exportSettings.ExtendedFileName)

            If File.Exists(fullPath) Then
                File.Delete(fullPath)
            End If
            File.WriteAllText(fullPath, json)
        End Sub

    End Class

End Namespace
