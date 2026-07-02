Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports MediaMaster.Data.Entities

Namespace Repositories

    Public Class MusicLogRepository
        Implements IMusicLogRepository

        Private ReadOnly _dbContextFactory As IDbContextFactory(Of MediaMasterDbContext)

        Public Sub New(dbContextFactory As IDbContextFactory(Of MediaMasterDbContext))
            _dbContextFactory = dbContextFactory
        End Sub

        Public Async Function AddAsync(entry As MusicLogEntry, Optional cancellationToken As CancellationToken = Nothing) As Task Implements IMusicLogRepository.AddAsync
            Using context = Await _dbContextFactory.CreateDbContextAsync(cancellationToken)
                context.MusicLog.Add(entry)
                Await context.SaveChangesAsync(cancellationToken)
            End Using
        End Function

    End Class

End Namespace
