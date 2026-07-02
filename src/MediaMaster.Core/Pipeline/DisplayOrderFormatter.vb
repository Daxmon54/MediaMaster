Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    ''' <summary>
    ''' Applies the 4-mode/2-separator "display order" setting to produce a single combined string.
    ''' In the original WinDev app this was computed in CreateFile() but never actually assigned to
    ''' any output (STC_StreamText and the export JSON used the raw artist/title instead) -- here it
    ''' is applied for real, to TrackInfo.Combined.
    ''' </summary>
    Public NotInheritable Class DisplayOrderFormatter

        Private Sub New()
        End Sub

        Public Shared Function Format(track As TrackInfo, settings As DisplayOrderSettings) As String
            Select Case settings.Mode
                Case DisplayOrderMode.ArtistTitle
                    Return $"{track.Artist} {settings.Separator1} {track.Title}"
                Case DisplayOrderMode.ArtistTitleYear
                    Return $"{track.Artist} {settings.Separator1} {track.Title} {settings.Separator2} {track.Year}"
                Case DisplayOrderMode.TitleArtist
                    Return $"{track.Title} {settings.Separator1} {track.Artist}"
                Case DisplayOrderMode.TitleArtistYear
                    Return $"{track.Title} {settings.Separator1} {track.Artist} {settings.Separator2} {track.Year}"
                Case Else
                    Return $"{track.Artist} {settings.Separator1} {track.Title}"
            End Select
        End Function

    End Class

End Namespace
