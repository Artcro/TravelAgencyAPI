using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportSearchNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitySearch",
                table: "Airports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountrySearch",
                table: "Airports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameSearch",
                table: "Airports",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Airports_CitySearch",
                table: "Airports",
                column: "CitySearch");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_CountrySearch",
                table: "Airports",
                column: "CountrySearch");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_NameSearch",
                table: "Airports",
                column: "NameSearch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Airports_CitySearch",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_CountrySearch",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_NameSearch",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "CitySearch",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "CountrySearch",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "NameSearch",
                table: "Airports");
        }
    }
}
