Imports System
Imports Microsoft.EntityFrameworkCore.Migrations
Imports Microsoft.VisualBasic

Namespace Global.MediaMaster.Data.Migrations
    ''' <inheritdoc />
    Partial Public Class InitialCreate
        Inherits Migration

        ''' <inheritdoc />
        Protected Overrides Sub Up(migrationBuilder As MigrationBuilder)
            migrationBuilder.CreateTable(
                name:="Editions",
                columns:=Function(table) New With {
                    .EditionsID = table.Column(Of Integer)(type:="INTEGER", nullable:=False).
                        Annotation("Sqlite:Autoincrement", True),
                    .Name = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Description = table.Column(Of String)(type:="TEXT", nullable:=True)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_Editions", Function(x) x.EditionsID)
                End Sub)

            migrationBuilder.CreateTable(
                name:="Filters",
                columns:=Function(table) New With {
                    .FiltersID = table.Column(Of Integer)(type:="INTEGER", nullable:=False).
                        Annotation("Sqlite:Autoincrement", True),
                    .FilterValue = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .NewValue = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Remark = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .ReplaceText = table.Column(Of Boolean)(type:="INTEGER", nullable:=False),
                    .PartOfText = table.Column(Of Boolean)(type:="INTEGER", nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_Filters", Function(x) x.FiltersID)
                End Sub)

            migrationBuilder.CreateTable(
                name:="MusicLog",
                columns:=Function(table) New With {
                    .MusicLogID = table.Column(Of Integer)(type:="INTEGER", nullable:=False).
                        Annotation("Sqlite:Autoincrement", True),
                    .DateStamp = table.Column(Of DateOnly)(type:="TEXT", nullable:=False),
                    .TimeStamp = table.Column(Of TimeOnly)(type:="TEXT", nullable:=False),
                    .DayNumber = table.Column(Of Integer)(type:="INTEGER", nullable:=False),
                    .EditionsID = table.Column(Of Integer)(type:="INTEGER", nullable:=False),
                    .Artist = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Title = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Album = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Label = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Isrc = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Year = table.Column(Of Integer)(type:="INTEGER", nullable:=False)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_MusicLog", Function(x) x.MusicLogID)
                End Sub)

            migrationBuilder.CreateTable(
                name:="TrafficInfo",
                columns:=Function(table) New With {
                    .TrafficInfoID = table.Column(Of Integer)(type:="INTEGER", nullable:=False).
                        Annotation("Sqlite:Autoincrement", True),
                    .SpotID = table.Column(Of Integer)(type:="INTEGER", nullable:=False),
                    .SpotName = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Web = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .RDS = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .Stream = table.Column(Of String)(type:="TEXT", nullable:=True),
                    .DAB = table.Column(Of String)(type:="TEXT", nullable:=True)
                },
                constraints:=Sub(table)
                    table.PrimaryKey("PK_TrafficInfo", Function(x) x.TrafficInfoID)
                End Sub)
        End Sub

        ''' <inheritdoc />
        Protected Overrides Sub Down(migrationBuilder As MigrationBuilder)
            migrationBuilder.DropTable(
                name:="Editions")

            migrationBuilder.DropTable(
                name:="Filters")

            migrationBuilder.DropTable(
                name:="MusicLog")

            migrationBuilder.DropTable(
                name:="TrafficInfo")
        End Sub
    End Class
End Namespace
