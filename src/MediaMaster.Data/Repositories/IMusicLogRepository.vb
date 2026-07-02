Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Data.Entities

Namespace Repositories

    Public Interface IMusicLogRepository
        Function AddAsync(entry As MusicLogEntry, Optional cancellationToken As CancellationToken = Nothing) As Task
    End Interface

End Namespace
