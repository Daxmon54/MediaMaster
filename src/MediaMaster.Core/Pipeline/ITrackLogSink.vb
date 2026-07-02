Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Monitoring

Namespace Pipeline

    ''' <summary>
    ''' Abstraction over the MusicLog insert (ACR Direct source only). Implemented in
    ''' MediaMaster.Data (which references Core) so that Core itself never depends on EF Core/Data.
    ''' </summary>
    Public Interface ITrackLogSink
        Function LogAsync(track As TrackInfo, editionId As Integer, cancellationToken As CancellationToken) As Task
    End Interface

End Namespace
