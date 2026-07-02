Imports System.IO
Imports System.Linq
Imports Microsoft.EntityFrameworkCore
Imports MediaMaster.Data
Imports MediaMaster.Data.Entities
Imports Xunit

Public Class DataLayerSmokeTests

    <Fact>
    Public Sub Migrate_And_RoundTrip_Edition_And_MusicLogEntry()
        Dim connectionString = $"Data Source={Path.Combine(Path.GetTempPath(), $"mm-smoke-{Guid.NewGuid()}.db")}"
        Dim options = New DbContextOptionsBuilder(Of MediaMasterDbContext)().
            UseSqlite(connectionString).
            Options

        Using writeContext = New MediaMasterDbContext(options)
            writeContext.Database.Migrate()
            writeContext.Editions.Add(New Edition With {.EditionsID = 7, .Name = "Drivetime", .Description = "16-19"})
            writeContext.MusicLog.Add(New MusicLogEntry With {
                .DateStamp = New DateOnly(2026, 7, 2),
                .TimeStamp = New TimeOnly(16, 30, 0),
                .DayNumber = 4,
                .EditionsID = 7,
                .Artist = "ABBA",
                .Title = "Dancing Queen",
                .Year = 1976
            })
            writeContext.SaveChanges()
        End Using

        Using readContext = New MediaMasterDbContext(options)
            Dim edition = readContext.Editions.Single(Function(e) e.EditionsID = 7)
            Assert.Equal("Drivetime", edition.Name)

            Dim logEntry = readContext.MusicLog.Single(Function(m) m.Artist = "ABBA")
            Assert.Equal("Dancing Queen", logEntry.Title)
            Assert.Equal(1976, logEntry.Year)
            Assert.Equal(7, logEntry.EditionsID)
        End Using
    End Sub

End Class
