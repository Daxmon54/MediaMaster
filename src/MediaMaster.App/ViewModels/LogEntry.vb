Namespace ViewModels

    Public Class LogEntry
        Public Property Timestamp As DateTime
        Public Property Text As String = String.Empty

        Public ReadOnly Property Display As String
            Get
                Return $"{Timestamp:HH:mm:ss} - {Text}"
            End Get
        End Property
    End Class

End Namespace
