Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline
Imports MediaMaster.Data.Entities

Namespace Repositories

    ''' <summary>Adapts Core's ITrackLogSink to a MusicLog insert via IMusicLogRepository.</summary>
    Public Class MusicLogTrackSink
        Implements ITrackLogSink

        Private ReadOnly _musicLogRepository As IMusicLogRepository

        Public Sub New(musicLogRepository As IMusicLogRepository)
            _musicLogRepository = musicLogRepository
        End Sub

        Public Function LogAsync(track As TrackInfo, editionId As Integer, cancellationToken As CancellationToken) As Task Implements ITrackLogSink.LogAsync
            Dim now = DateTime.Now
            Dim parsedYear As Integer
            Integer.TryParse(track.Year, parsedYear)

            Dim entry As New MusicLogEntry With {
                .DateStamp = DateOnly.FromDateTime(now),
                .TimeStamp = TimeOnly.FromDateTime(now),
                .DayNumber = CInt(now.DayOfWeek),
                .EditionsID = editionId,
                .Artist = track.Artist,
                .Title = track.Title,
                .Album = track.Album,
                .Label = track.Label,
                .Isrc = track.Isrc,
                .Year = parsedYear
            }
            Return _musicLogRepository.AddAsync(entry, cancellationToken)
        End Function

    End Class

End Namespace
