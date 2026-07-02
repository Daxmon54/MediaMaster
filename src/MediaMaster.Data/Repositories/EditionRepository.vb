Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports MediaMaster.Data.Entities

Namespace Repositories

    ''' <summary>
    ''' Uses IDbContextFactory rather than an injected DbContext directly: DbContext isn't thread-safe,
    ''' and this repository may be resolved into long-lived (singleton) services shared across several
    ''' concurrently-running source monitors -- a factory means each call gets its own short-lived context.
    ''' </summary>
    Public Class EditionRepository
        Implements IEditionRepository

        Private ReadOnly _dbContextFactory As IDbContextFactory(Of MediaMasterDbContext)

        Public Sub New(dbContextFactory As IDbContextFactory(Of MediaMasterDbContext))
            _dbContextFactory = dbContextFactory
        End Sub

        Public Async Function GetByIdAsync(editionsId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Edition) Implements IEditionRepository.GetByIdAsync
            Using context = Await _dbContextFactory.CreateDbContextAsync(cancellationToken)
                Return Await context.Editions.FirstOrDefaultAsync(Function(e) e.EditionsID = editionsId, cancellationToken)
            End Using
        End Function

        Public Async Function GetAllAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Edition)) Implements IEditionRepository.GetAllAsync
            Using context = Await _dbContextFactory.CreateDbContextAsync(cancellationToken)
                Return Await context.Editions.OrderBy(Function(e) e.Name).ToListAsync(cancellationToken)
            End Using
        End Function

    End Class

End Namespace
