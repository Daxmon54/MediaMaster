Namespace Polling

    ''' <summary>Implemented in MediaMaster.App to marshal Core's output onto the UI thread.</summary>
    Public Interface IUiUpdateSink
        Sub UpdateStatus(statusText As String)
        Sub AppendLog(logLine As String)
        Sub SetProgress(current As Integer, maximum As Integer)
    End Interface

End Namespace
