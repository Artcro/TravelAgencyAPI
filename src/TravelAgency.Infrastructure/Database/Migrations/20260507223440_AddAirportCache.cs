using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirportDataSyncStatuses",
                columns: table => new
                {
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastSucceededAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImportedAirportCount = table.Column<int>(type: "integer", nullable: false),
                    SourceRecordCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportDataSyncStatuses", x => x.Source);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    IataCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IcaoCode = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    Ident = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CountryName = table.Column<string>(type: "text", nullable: false),
                    AirportType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScheduledService = table.Column<bool>(type: "boolean", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.IataCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airports_City",
                table: "Airports",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_CountryCode",
                table: "Airports",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_IsActive",
                table: "Airports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_LastSyncedAtUtc",
                table: "Airports",
                column: "LastSyncedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_Name",
                table: "Airports",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirportDataSyncStatuses");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
