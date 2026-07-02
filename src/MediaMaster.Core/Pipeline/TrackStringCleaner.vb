Imports System.Text.RegularExpressions

Namespace Pipeline

    Public NotInheritable Class TrackStringCleaner

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Trims outer whitespace and, when enabled, replaces "&amp;" with "-" (e.g. "Simon & Garfunkel"
        ''' -&gt; "Simon - Garfunkel"). The original WLanguage code stripped ALL internal spaces in this
        ''' case (NoSpace() with no scope argument), which would mangle any multi-word artist/title
        ''' ("Simon & Garfunkel" -&gt; "SimonGarfunkel") -- that looks like a latent bug rather than
        ''' intended behavior, so here we only collapse runs of whitespace instead of removing it.
        ''' </summary>
        Public Shared Function Clean(value As String, Optional replaceAmpersand As Boolean = False) As String
            If String.IsNullOrEmpty(value) Then
                Return String.Empty
            End If

            Dim result = value
            If replaceAmpersand Then
                result = result.Replace("&", "-")
            End If

            result = Regex.Replace(result, "\s+", " ")
            Return result.Trim()
        End Function

    End Class

End Namespace
