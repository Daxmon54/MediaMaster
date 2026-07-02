Imports System.Net.Http
Imports System.Windows.Threading
Imports Microsoft.EntityFrameworkCore
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports MediaMaster.App.Services
Imports MediaMaster.App.ViewModels
Imports MediaMaster.App.Views
Imports MediaMaster.Core.Configuration
Imports MediaMaster.Core.Monitoring
Imports MediaMaster.Core.Pipeline
Imports MediaMaster.Core.Polling
Imports MediaMaster.Data
Imports MediaMaster.Data.Repositories

Partial Public Class Application
    Inherits System.Windows.Application

    Public Property Host As IHost

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)

        Host = CreateHostBuilder().Build()
        Host.Start()

        Using dbContext = Host.Services.GetRequiredService(Of IDbContextFactory(Of MediaMasterDbContext))().CreateDbContext()
            dbContext.Database.Migrate()
        End Using

        Dim mainWindow = Host.Services.GetRequiredService(Of MainWindow)()
        mainWindow.Show()
    End Sub

    Protected Overrides Sub OnExit(e As ExitEventArgs)
        Host?.Dispose()
        MyBase.OnExit(e)
    End Sub

    Private Shared Function CreateHostBuilder() As IHostBuilder
        Return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().
            ConfigureAppConfiguration(
                Sub(context, config)
                    config.SetBasePath(AppContext.BaseDirectory)
                    config.AddJsonFile("appsettings.json", optional:=True, reloadOnChange:=False)
                End Sub).
            ConfigureServices(
                Sub(context, services)
                    ConfigureServices(services, context.Configuration)
                End Sub)
    End Function

    Private Shared Sub ConfigureServices(services As IServiceCollection, configuration As IConfiguration)
        services.AddSingleton(Of IConfiguration)(configuration)
        services.AddSingleton(Of IAppSettingsProvider, AppSettingsProvider)()

        Dim connectionString = configuration.GetValue(Of String)("ConnectionString", "Data Source=mediamaster.db")
        services.AddDbContextFactory(Of MediaMasterDbContext)(Sub(options) options.UseSqlite(connectionString))

        services.AddSingleton(Of IEditionRepository, EditionRepository)()
        services.AddSingleton(Of IMusicLogRepository, MusicLogRepository)()
        services.AddSingleton(Of ITrackLogSink, MusicLogTrackSink)()

        services.AddSingleton(Of IExportFileWriter, ExportFileWriter)()
        services.AddSingleton(Of IPlaylogWriter, PlaylogWriter)()
        services.AddSingleton(Of ILiveTrackWriter, LiveTrackWriter)()
        services.AddSingleton(Of ITrackPublisher, TrackPublisher)()

        services.AddSingleton(Of MainViewModel)()
        services.AddSingleton(Of Dispatcher)(Function(sp) Dispatcher.CurrentDispatcher)
        services.AddSingleton(Of IUiUpdateSink, DispatcherUiUpdateService)()
        services.AddSingleton(Of MainWindow)()

        services.AddTransient(Of SettingsViewModel)()
        services.AddTransient(Of SettingsWindow)()
        services.AddSingleton(Of Func(Of SettingsWindow))(Function(sp) Function() sp.GetRequiredService(Of SettingsWindow)())

        services.AddSingleton(Of HttpClient)()
        services.AddSingleton(Of ISourceMonitorFactory, SourceMonitorFactory)()
        services.AddSingleton(Of ISourceMonitor)(Function(sp) sp.GetRequiredService(Of ISourceMonitorFactory)().Create())
        services.AddHostedService(Of PollingCoordinator)()
        services.AddHostedService(Of SourceMonitorHostedService)()
    End Sub

End Class
