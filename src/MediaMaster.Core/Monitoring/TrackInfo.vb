Namespace Monitoring

    Public Class TrackInfo
        Public Property Artist As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Year As String = String.Empty
        Public Property Label As String = String.Empty
        Public Property Album As String = String.Empty
        Public Property Isrc As String = String.Empty
        Public Property InfoType As Integer
        ''' <summary>Formatted "MM:SS"-style duration, e.g. from a track-length lookup. Empty when unknown.</summary>
        Public Property Duration As String = String.Empty
        ''' <summary>Artist/title (/year) combined per the configured display order; set by DisplayOrderFormatter.</summary>
        Public Property Combined As String = String.Empty
    End Class

End Namespace
