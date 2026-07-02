Imports System.IO
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    Public Interface IPlaylogWriter
        Sub Append(track As TrackInfo, tracklogSettings As TracklogSettings)
    End Interface

    ''' <summary>Appends one line per track to a daily "YYYY-MM-DD-playlog.txt" file.</summary>
    Public Class PlaylogWriter
        Implements IPlaylogWriter

        Public Sub Append(track As TrackInfo, tracklogSettings As TracklogSettings) Implements IPlaylogWriter.Append
            If String.IsNullOrEmpty(tracklogSettings.FolderPath) Then
                Return
            End If

            If Not Directory.Exists(tracklogSettings.FolderPath) Then
                Directory.CreateDirectory(tracklogSettings.FolderPath)
            End If

            Dim fileName = $"{DateTime.Now:yyyy-MM-dd}-playlog.txt"
            Dim fullPath = Path.Combine(tracklogSettings.FolderPath, fileName)
            Dim line = $"{DateTime.Now:HH:mm:ss} {track.Artist} - {track.Title}"

            File.AppendAllLines(fullPath, {line})
        End Sub

    End Class

End Namespace
