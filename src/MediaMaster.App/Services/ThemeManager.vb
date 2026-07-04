Imports System.Windows
Imports MediaMaster.Core.Configuration

Namespace Services

    ''' <summary>
    ''' Applies a light/dark theme by swapping a theme ResourceDictionary into the application's
    ''' merged dictionaries. The theme dictionaries hold implicit (type-keyed) styles, so all open
    ''' and future windows pick up the change automatically without any per-window wiring.
    ''' </summary>
    Public NotInheritable Class ThemeManager

        Private Shared _currentThemeDictionary As ResourceDictionary

        Private Sub New()
        End Sub

        Public Shared Sub Apply(theme As AppTheme)
            Dim source As String
            Select Case theme
                Case AppTheme.Dark
                    source = "/MediaMaster.App;component/Themes/Dark.xaml"
                Case Else
                    source = "/MediaMaster.App;component/Themes/Light.xaml"
            End Select

            Dim newDictionary As New ResourceDictionary With {
                .Source = New Uri(source, UriKind.Relative)
            }

            Dim mergedDictionaries = Application.Current.Resources.MergedDictionaries
            If _currentThemeDictionary IsNot Nothing AndAlso mergedDictionaries.Contains(_currentThemeDictionary) Then
                mergedDictionaries.Remove(_currentThemeDictionary)
            End If
            mergedDictionaries.Add(newDictionary)
            _currentThemeDictionary = newDictionary
        End Sub

    End Class

End Namespace
