Imports System.Windows
Imports MediaMaster.Core.Configuration

Namespace Services

    ''' <summary>
    ''' Applies a light/dark theme by swapping the active palette ResourceDictionary (brushes only)
    ''' into the application's merged dictionaries. The control styles/templates in Controls.xaml
    ''' (merged permanently in App.xaml) reference those brushes via DynamicResource, so swapping the
    ''' palette restyles every open and future window live -- no per-window wiring needed.
    ''' </summary>
    Public NotInheritable Class ThemeManager

        Private Shared _currentPalette As ResourceDictionary

        Private Sub New()
        End Sub

        Public Shared Sub Apply(theme As AppTheme)
            Dim source As String
            Select Case theme
                Case AppTheme.Dark
                    source = "/MediaMaster.App;component/Themes/Palette.Dark.xaml"
                Case Else
                    source = "/MediaMaster.App;component/Themes/Palette.Light.xaml"
            End Select

            Dim newPalette As New ResourceDictionary With {
                .Source = New Uri(source, UriKind.Relative)
            }

            Dim mergedDictionaries = Application.Current.Resources.MergedDictionaries
            If _currentPalette IsNot Nothing AndAlso mergedDictionaries.Contains(_currentPalette) Then
                mergedDictionaries.Remove(_currentPalette)
            End If
            mergedDictionaries.Add(newPalette)
            _currentPalette = newPalette
        End Sub

    End Class

End Namespace
