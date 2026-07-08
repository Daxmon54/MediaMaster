Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Enrichment
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Polling

Namespace Pipeline

    ''' <summary>
    ''' The CreateFile() replacement shared by all 6 source monitors.
    '''
    ''' Fixes two bugs present in the original WinDev CreateFile(): (1) the original re-derived
    ''' Artist/Title from two locals that were declared but never assigned right before writing the
    ''' export/status, silently blanking both for the Carmen and ACR-FTP sources -- here the actually
    ''' parsed/cleaned values are always what gets published; (2) the display-order setting is applied
    ''' to TrackInfo.Combined instead of being computed and discarded.
    ''' </summary>
    Public Class TrackPublisher
        Implements ITrackPublisher

        ''' <summary>How long to wait for the online casing lookup before publishing anyway (correctness over speed, but bounded).</summary>
        Private Shared ReadOnly CasingLookupTimeout As TimeSpan = TimeSpan.FromSeconds(3)

        Private ReadOnly _settingsProvider As IAppSettingsProvider
        Private ReadOnly _exportWriter As IExportFileWriter
        Private ReadOnly _playlogWriter As IPlaylogWriter
        Private ReadOnly _liveTrackWriter As ILiveTrackWriter
        Private ReadOnly _uiSink As IUiUpdateSink
        Private ReadOnly _trackLogSink As ITrackLogSink
        Private ReadOnly _nameResolver As ITrackNameResolver

        Public Sub New(settingsProvider As IAppSettingsProvider,
                        exportWriter As IExportFileWriter,
                        playlogWriter As IPlaylogWriter,
                        liveTrackWriter As ILiveTrackWriter,
                        uiSink As IUiUpdateSink,
                        Optional trackLogSink As ITrackLogSink = Nothing,
                        Optional nameResolver As ITrackNameResolver = Nothing)
            _settingsProvider = settingsProvider
            _exportWriter = exportWriter
            _playlogWriter = playlogWriter
            _liveTrackWriter = liveTrackWriter
            _uiSink = uiSink
            _trackLogSink = trackLogSink
            _nameResolver = nameResolver
        End Sub

        Public Async Function PublishAsync(sourceLabel As String, rawTrack As TrackInfo, logToDatabase As Boolean, cancellationToken As CancellationToken) As Task(Of TrackInfo) Implements ITrackPublisher.PublishAsync
            Dim settings = _settingsProvider.GetSettings()

            Dim cleaned As New TrackInfo With {
                .Artist = TrackStringCleaner.Clean(rawTrack.Artist, settings.ReplaceAmpersand),
                .Title = TrackStringCleaner.Clean(rawTrack.Title, settings.ReplaceAmpersand),
                .Year = rawTrack.Year,
                .Label = rawTrack.Label,
                .Album = rawTrack.Album,
                .Isrc = rawTrack.Isrc,
                .InfoType = rawTrack.InfoType,
                .Duration = rawTrack.Duration
            }

            If String.IsNullOrEmpty(cleaned.Artist) AndAlso String.IsNullOrEmpty(cleaned.Title) Then
                Return cleaned
            End If

            If settings.CorrectTrackCasing AndAlso _nameResolver IsNot Nothing Then
                Await ApplyOnlineCasingAsync(cleaned, cancellationToken)
            End If

            cleaned.Combined = DisplayOrderFormatter.Format(cleaned, settings.DisplayOrder)

            _exportWriter.Write(cleaned, settings.Export)
            _uiSink.UpdateStatus($"{cleaned.Artist} - {cleaned.Title} - {cleaned.Year}")
            _uiSink.AppendLog($"{sourceLabel} : {cleaned.Artist} - {cleaned.Title} - {cleaned.Year}")

            If settings.Tracklog.LogToFile Then
                _playlogWriter.Append(cleaned, settings.Tracklog)
            End If

            If settings.LiveTrack.Enabled Then
                _liveTrackWriter.Write(cleaned, settings.LiveTrack)
            End If

            If logToDatabase AndAlso settings.Tracklog.LogToDatabase AndAlso _trackLogSink IsNot Nothing Then
                Await _trackLogSink.LogAsync(cleaned, settings.EditionId, cancellationToken)
            End If

            Return cleaned
        End Function

        ''' <summary>
        ''' Replaces the track's artist/title with the canonical spelling from the online music
        ''' database, bounded by a short timeout. A non-match, error, or timeout leaves them unchanged.
        ''' </summary>
        Private Async Function ApplyOnlineCasingAsync(track As TrackInfo, cancellationToken As CancellationToken) As Task
            Using timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                timeoutSource.CancelAfter(CasingLookupTimeout)
                Dim resolved = Await _nameResolver.ResolveAsync(track.Artist, track.Title, timeoutSource.Token)
                If resolved IsNot Nothing AndAlso resolved.Matched Then
                    track.Artist = resolved.Artist
                    track.Title = resolved.Title
                End If
            End Using
        End Function

    End Class

End Namespace
