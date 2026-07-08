Imports System.Threading
Imports System.Threading.Tasks

Namespace Enrichment

    ''' <summary>Canonical artist/title as resolved from an online music database; Matched is False when no confident match was found.</summary>
    Public Class ResolvedName
        Public Property Artist As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Matched As Boolean
    End Class

    ''' <summary>Looks up the canonical (correctly-cased) spelling of an artist/title from an online music database.</summary>
    Public Interface ITrackNameResolver
        Function ResolveAsync(artist As String, title As String, cancellationToken As CancellationToken) As Task(Of ResolvedName)
    End Interface

End Namespace
