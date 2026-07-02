Imports Microsoft.EntityFrameworkCore
Imports MediaMaster.Data.Entities

Public Class MediaMasterDbContext
    Inherits DbContext

    Public Sub New(options As DbContextOptions(Of MediaMasterDbContext))
        MyBase.New(options)
    End Sub

    Public Property Editions As DbSet(Of Edition)
    Public Property MusicLog As DbSet(Of MusicLogEntry)
    Public Property TrafficInfo As DbSet(Of TrafficInfo)
    Public Property Filters As DbSet(Of Filter)

    Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
        modelBuilder.Entity(Of Edition)().HasKey(Function(e) e.EditionsID)
        modelBuilder.Entity(Of MusicLogEntry)().HasKey(Function(m) m.MusicLogID)
        modelBuilder.Entity(Of TrafficInfo)().HasKey(Function(t) t.TrafficInfoID)
        modelBuilder.Entity(Of Filter)().HasKey(Function(f) f.FiltersID)
    End Sub

End Class
