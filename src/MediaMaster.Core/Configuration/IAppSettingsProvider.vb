Namespace Configuration

    Public Interface IAppSettingsProvider
        Function GetSettings() As AppSettings
        ''' <summary>Persists the current (possibly just-mutated) settings back to disk.</summary>
        Sub Save()
    End Interface

End Namespace
