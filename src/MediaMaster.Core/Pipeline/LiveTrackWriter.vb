Imports System.IO
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    Public Interface ILiveTrackWriter
        Sub Write(track As TrackInfo, liveTrackSettings As LiveTrackSettings)
    End Interface

    ''' <summary>Overwrites a single-line "livetrack.txt" with the current artist/title.</summary>
    Public Class LiveTrackWriter
        Implements ILiveTrackWriter

        Public Sub Write(track As TrackInfo, liveTrackSettings As LiveTrackSettings) Implements ILiveTrackWriter.Write
            If String.IsNullOrEmpty(liveTrackSettings.FolderPath) Then
                Return
            End If

            If Not Directory.Exists(liveTrackSettings.FolderPath) Then
                Directory.CreateDirectory(liveTrackSettings.FolderPath)
            End If

            Dim text As String
            If Not String.IsNullOrEmpty(track.Title) Then
                text = $"{track.Artist} - {track.Title}"
            Else
                text = track.Artist
            End If

            Dim fullPath = Path.Combine(liveTrackSettings.FolderPath, "livetrack.txt")
            File.WriteAllText(fullPath, text)
        End Sub

    End Class

End Namespace
