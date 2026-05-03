using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelAgency.Infrastructure.Database.Migrations;

public partial class AddMissingAppTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS "TripSearches" (
    "Id" uuid NOT NULL,
    "UserId" uuid NULL,
    "Origin" text NOT NULL,
    "Destination" text NOT NULL,
    "DepartureDate" date NOT NULL,
    "ReturnDate" date NULL,
    "Adults" integer NOT NULL,
    "Children" integer NOT NULL,
    "Infants" integer NOT NULL,
    "Currency" text NOT NULL,
    "RequestJson" jsonb NOT NULL,
    "ResponseJson" jsonb NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "ProviderStatus" text NOT NULL,
    CONSTRAINT "PK_TripSearches" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "SavedTrips" (
    "Id" uuid NOT NULL,
    "UserId" uuid NULL,
    "SearchId" uuid NOT NULL,
    "Name" text NOT NULL,
    "SelectedFlightProviderOfferId" text NULL,
    "SelectedHotelProviderHotelId" text NULL,
    "SelectedActivityIdsJson" jsonb NOT NULL,
    "Status" text NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "UpdatedAtUtc" timestamp with time zone NULL,
    CONSTRAINT "PK_SavedTrips" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SavedTrips_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SavedTrips_TripSearches_SearchId" FOREIGN KEY ("SearchId") REFERENCES "TripSearches" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" uuid NOT NULL,
    "UserId" uuid NULL,
    "Action" text NOT NULL,
    "ResourceType" text NOT NULL,
    "ResourceId" text NULL,
    "IpAddress" text NULL,
    "UserAgent" text NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "MetadataJson" jsonb NULL,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_SavedTrips_UserId" ON "SavedTrips" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_SavedTrips_SearchId" ON "SavedTrips" ("SearchId");
CREATE INDEX IF NOT EXISTS "IX_SavedTrips_CreatedAtUtc" ON "SavedTrips" ("CreatedAtUtc");
CREATE INDEX IF NOT EXISTS "IX_TripSearches_CreatedAtUtc" ON "TripSearches" ("CreatedAtUtc");
CREATE INDEX IF NOT EXISTS "IX_ProviderRequestLogs_CreatedAtUtc" ON "ProviderRequestLogs" ("CreatedAtUtc");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_CreatedAtUtc" ON "AuditLogs" ("CreatedAtUtc");
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
