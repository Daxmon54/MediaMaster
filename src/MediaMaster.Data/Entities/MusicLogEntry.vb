Namespace Entities

    ''' <summary>Insert-only log of tracks detected via the ACR Direct source.</summary>
    Public Class MusicLogEntry
        Public Property MusicLogID As Integer
        Public Property DateStamp As DateOnly
        Public Property TimeStamp As TimeOnly
        Public Property DayNumber As Integer
        Public Property EditionsID As Integer
        Public Property Artist As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Album As String = String.Empty
        Public Property Label As String = String.Empty
        Public Property Isrc As String = String.Empty
        Public Property Year As Integer
    End Class

End Namespace
