Imports System.IO

Namespace Monitoring

    ''' <summary>
    ''' Original CheckPFL(): a "protected content" semaphore -- while a PFL_A.SEM or PFL_B.SEM file
    ''' exists in the configured folder, publishing is suppressed. In the original, only the Carmen
    ''' and OTS paths call this (the Caliope/PowerStudio paths had the check commented out), so it's
    ''' only wired into CarmenUdpMonitor and OtsFileMonitor here too.
    ''' </summary>
    Public Module PflGate

        Public Function IsSuppressed(pflFolder As String) As Boolean
            If String.IsNullOrEmpty(pflFolder) OrElse Not Directory.Exists(pflFolder) Then
                Return False
            End If
            Return File.Exists(Path.Combine(pflFolder, "PFL_A.SEM")) OrElse File.Exists(Path.Combine(pflFolder, "PFL_B.SEM"))
        End Function

    End Module

End Namespace
