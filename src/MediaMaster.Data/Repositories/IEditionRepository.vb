Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Data.Entities

Namespace Repositories

    Public Interface IEditionRepository
        Function GetByIdAsync(editionsId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Edition)
    End Interface

End Namespace
