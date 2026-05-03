using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Infrastructure.Database.Migrations;

public partial class AddMissingProviderRequestLogsRepair : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS "ProviderRequestLogs" (
    "Id" uuid NOT NULL,
    "Provider" text NOT NULL,
    "Endpoint" text NOT NULL,
    "StatusCode" integer NULL,
    "Success" boolean NOT NULL,
    "ErrorMessage" text NULL,
    "DurationMs" bigint NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProviderRequestLogs" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "IX_ProviderRequestLogs_CreatedAtUtc" ON "ProviderRequestLogs" ("CreatedAtUtc");
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
