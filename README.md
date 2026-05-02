# TravelAgencyApi

## Phase 1 status
Implemented foundational backend structure for database + Identity + JWT wiring + auth controller skeleton.

## Run PostgreSQL with Docker Compose
```bash
docker compose up -d postgres
```

## Restore/build/test
```bash
dotnet restore
dotnet build TravelAgencyApi.sln
dotnet test TravelAgencyApi.sln
```

> If your environment blocks NuGet (`https://api.nuget.org`) via proxy (403), run these in a machine with NuGet access.

## Run API
```bash
dotnet run --project src/TravelAgency.Api/TravelAgency.Api.csproj
```

## Secrets configuration (later)
Use user-secrets or env vars for:
- `Jwt:Secret`
- `Amadeus:ClientId`
- `Amadeus:ClientSecret`
