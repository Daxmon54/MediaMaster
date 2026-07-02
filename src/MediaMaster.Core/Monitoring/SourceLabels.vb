Namespace Monitoring

    ''' <summary>Log-line source prefixes, matching the original WinDev LogIt() call sites ("CARMEN : ...", "ACR : ...", etc.).</summary>
    Public Module SourceLabels

        Public Function ForSystemType(systemType As Integer) As String
            Select Case systemType
                Case 1
                    Return "CARMEN"
                Case 2, 3
                    Return "ACR"
                Case 4
                    Return "OTS"
                Case 5
                    Return "Caliope"
                Case 6
                    Return "PowerStudio"
                Case Else
                    Return "UNKNOWN"
            End Select
        End Function

    End Module

End Namespace
