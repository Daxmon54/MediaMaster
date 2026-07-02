Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    Public Interface ITrackPublisher
        ''' <summary>
        ''' Cleans, formats, and publishes a raw track: writes the export JSON, updates the UI status/log,
        ''' optionally appends to the playlog / writes the live-track file, and (when requested and enabled)
        ''' logs to the MusicLog table. Returns the cleaned/finalized TrackInfo.
        ''' </summary>
        Function PublishAsync(sourceLabel As String, rawTrack As TrackInfo, logToDatabase As Boolean, cancellationToken As CancellationToken) As Task(Of TrackInfo)
    End Interface

End Namespace
