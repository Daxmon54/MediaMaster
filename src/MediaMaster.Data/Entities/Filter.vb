Namespace Entities

    ''' <summary>
    ''' Entity shape only. In the original WinDev app, the procedure that used this table
    ''' (FilterCheck) had an empty loop body and never actually implemented matching logic
    ''' (both the "part of text" and "full text" branches were unfinished), so no filter
    ''' engine is ported here -- just the schema.
    ''' </summary>
    Public Class Filter
        Public Property FiltersID As Integer
        Public Property FilterValue As String = String.Empty
        Public Property NewValue As String = String.Empty
        Public Property Remark As String = String.Empty
        Public Property ReplaceText As Boolean
        Public Property PartOfText As Boolean
    End Class

End Namespace
