using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Destino = table.Column<string>(type: "text", nullable: false),
                    Imagem = table.Column<string>(type: "text", nullable: true),
                    DataViagem = table.Column<string>(type: "text", nullable: false),
                    Viajantes = table.Column<int>(type: "integer", nullable: false),
                    CiaAerea = table.Column<string>(type: "text", nullable: true),
                    Hotel = table.Column<string>(type: "text", nullable: true),
                    HotelValor = table.Column<decimal>(type: "numeric", nullable: true),
                    Carro = table.Column<string>(type: "text", nullable: true),
                    CarroValor = table.Column<decimal>(type: "numeric", nullable: true),
                    Passeio = table.Column<string>(type: "text", nullable: true),
                    PasseioValor = table.Column<decimal>(type: "numeric", nullable: true),
                    ValorVoo = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelPackages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_CreatedAtUtc",
                table: "TravelPackages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TravelPackages_UserId",
                table: "TravelPackages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelPackages");
        }
    }
}
