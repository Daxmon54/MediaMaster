Imports MediaMaster.Core.Pipeline
Imports Xunit

Public Class TrackStringCleanerTests

    <Theory>
    <InlineData("  ABBA  ", False, "ABBA")>
    <InlineData("Simon & Garfunkel", False, "Simon & Garfunkel")>
    <InlineData("Simon & Garfunkel", True, "Simon - Garfunkel")>
    <InlineData("Multiple   Internal   Spaces", True, "Multiple Internal Spaces")>
    <InlineData("", True, "")>
    <InlineData(Nothing, True, "")>
    Public Sub Clean_TrimsAndOptionallyReplacesAmpersand(input As String, replaceAmpersand As Boolean, expected As String)
        Dim result = TrackStringCleaner.Clean(input, replaceAmpersand)

        Assert.Equal(expected, result)
    End Sub

    <Fact>
    Public Sub Clean_DoesNotStripInternalSpaces_WhenAmpersandReplaced()
        ' Regression guard: the original WLanguage NoSpace() (no scope arg) strips ALL spaces,
        ' which would turn "Simon & Garfunkel" into "SimonGarfunkel". That's not replicated here.
        Dim result = TrackStringCleaner.Clean("Simon & Garfunkel", replaceAmpersand:=True)

        Assert.Equal("Simon - Garfunkel", result)
        Assert.Contains(" ", result)
    End Sub

End Class
