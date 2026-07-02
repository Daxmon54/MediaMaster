Namespace Entities

    ''' <summary>
    ''' Entity shape only. In the original WinDev app, the procedure that read this table
    ''' (ZoekSpot) only produced debug trace output and was never wired into the main
    ''' polling flow, so no query/lookup logic is ported here -- just the schema.
    ''' </summary>
    Public Class TrafficInfo
        Public Property TrafficInfoID As Integer
        Public Property SpotID As Integer
        Public Property SpotName As String = String.Empty
        Public Property Web As String = String.Empty
        Public Property RDS As String = String.Empty
        Public Property Stream As String = String.Empty
        Public Property DAB As String = String.Empty
    End Class

End Namespace
