Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports System.Threading.Tasks
Imports MediaMaster.Core.Configuration

Namespace Monitoring

    ''' <summary>
    ''' Polls the ACRCloud "realtime_results" REST endpoint. Unlike the other 5 sources, this one
    ''' also drives a MusicLog database insert -- that's wired at the SourceMonitorHostedService/
    ''' TrackPublisher level (gated on AppSettings.SystemType = 2), not inside this class, to keep
    ''' the DB-log decision in one place instead of duplicated per-monitor.
    ''' </summary>
    Public Class AcrCloudDirectMonitor
        Implements ISourceMonitor

        Public Event TrackChanged As EventHandler(Of TrackInfo) Implements ISourceMonitor.TrackChanged

        Private ReadOnly _settings As AcrDirectSettings
        Private ReadOnly _httpClient As HttpClient
        Private _cts As CancellationTokenSource
        Private _pollTask As Task
        Private _lastTrackKey As String = String.Empty

        Public Sub New(settings As AcrDirectSettings, httpClient As HttpClient)
            _settings = settings
            _httpClient = httpClient
        End Sub

        Public ReadOnly Property PollingIntervalMs As Integer Implements ISourceMonitor.PollingIntervalMs
            Get
                Return Math.Max(_settings.PollIntervalSeconds, 1) * 1000
            End Get
        End Property

        Public Function StartAsync(cancellationToken As CancellationToken) As Task Implements ISourceMonitor.StartAsync
            If String.IsNullOrEmpty(_settings.BaseUrl) Then
                Throw New InvalidOperationException("AcrDirectSettings.BaseUrl is not configured.")
            End If

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            _pollTask = Task.Run(AddressOf PollLoopAsync)
            Return Task.CompletedTask
        End Function

        Public Async Function StopAsync() As Task Implements ISourceMonitor.StopAsync
            _cts?.Cancel()
            If _pollTask IsNot Nothing Then
                Try
                    Await _pollTask
                Catch ex As OperationCanceledException
                End Try
            End If
            _cts?.Dispose()
        End Function

        Private Async Function PollLoopAsync() As Task
            Try
                Using timer As New PeriodicTimer(TimeSpan.FromSeconds(Math.Max(_settings.PollIntervalSeconds, 1)))
                    Do
                        Await PollOnceAsync(_cts.Token)
                    Loop While Await timer.WaitForNextTickAsync(_cts.Token)
                End Using
            Catch ex As OperationCanceledException
                ' expected during shutdown
            End Try
        End Function

        Private Async Function PollOnceAsync(cancellationToken As CancellationToken) As Task
            Try
                Dim url = $"{_settings.BaseUrl}{_settings.ProjectId}/channels/{_settings.ChannelId}/realtime_results"
                Using request As New HttpRequestMessage(HttpMethod.Get, url)
                    request.Headers.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
                    request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", _settings.Token?.Trim())

                    Using response = Await _httpClient.SendAsync(request, cancellationToken)
                        response.EnsureSuccessStatusCode()
                        Dim rawJson = Await response.Content.ReadAsStringAsync(cancellationToken)

                        Dim track = AcrDirectResponseParser.Parse(rawJson)
                        If track Is Nothing Then
                            Return
                        End If

                        Dim trackKey = $"{track.Artist}|{track.Title}"
                        If trackKey = _lastTrackKey Then
                            Return
                        End If
                        _lastTrackKey = trackKey

                        RaiseEvent TrackChanged(Me, track)
                    End Using
                End Using
            Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException)
                ' network hiccups (host unreachable, auth failure, malformed response, etc.)
                ' are expected for a REST poll -- just try again next tick.
            End Try
        End Function

        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
            Return New ValueTask(StopAsync())
        End Function

    End Class

End Namespace
