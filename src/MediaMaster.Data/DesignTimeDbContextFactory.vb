Imports Microsoft.EntityFrameworkCore
Imports Microsoft.EntityFrameworkCore.Design

''' <summary>Used only by `dotnet ef migrations add` at design time; the app itself builds options via DI.</summary>
Public Class DesignTimeDbContextFactory
    Implements IDesignTimeDbContextFactory(Of MediaMasterDbContext)

    Public Function CreateDbContext(args As String()) As MediaMasterDbContext Implements IDesignTimeDbContextFactory(Of MediaMasterDbContext).CreateDbContext
        Dim optionsBuilder As New DbContextOptionsBuilder(Of MediaMasterDbContext)()
        optionsBuilder.UseSqlite("Data Source=mediamaster.db")
        Return New MediaMasterDbContext(optionsBuilder.Options)
    End Function

End Class
