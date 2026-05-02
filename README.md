# TravelAgencyApi

## Phase 3 Status
Phase 3 is implemented: migrations setup instructions, saved-trip ownership hardening, service-level test expansion, Amadeus parsing hardening, rate limiting policies, and README operational guidance are in place.

## Database and Migrations
Run from repository root:

```bash
dotnet ef migrations add InitialCreate \
  --project src/TravelAgency.Infrastructure \
  --startup-project src/TravelAgency.Api \
  --output-dir Database/Migrations

# apply migrations
dotnet ef database update \
  --project src/TravelAgency.Infrastructure \
  --startup-project src/TravelAgency.Api

# list migrations
dotnet ef migrations list \
  --project src/TravelAgency.Infrastructure \
  --startup-project src/TravelAgency.Api
```

> Note: if `dotnet ef` is missing, install it first: `dotnet tool install --global dotnet-ef`.

## Security: RequireAuthentication
`appsettings.json`:

```json
"Security": {
  "RequireAuthentication": false
}
```

- `false` (demo mode): saved trips may be created anonymously; `SavedTripEntity.UserId` can be null; list endpoints can return anonymous/demo trips.
- `true` (secured mode): saved-trip operations require authenticated context, and all list/get/delete operations are constrained to the signed-in owner.

## Anonymous vs Authenticated Saved Trips
- Demo mode keeps frontend demo behavior and allows anonymous save/list/delete flows.
- Authenticated mode enforces ownership boundaries for create/list/get/delete and performs soft delete with audit logging.

## Rate Limiting
Named policies:
- `auth-strict` for `/api/v1/auth/login`, `/api/v1/auth/register`, `/api/v1/auth/refresh`
- `search-medium` for `/api/v1/trips/search`
- `locations-relaxed` for `/api/v1/locations`

## Local Build/Test
```bash
dotnet restore TravelAgencyApi.sln
dotnet build TravelAgencyApi.sln
dotnet test TravelAgencyApi.sln
```

## Amadeus Setup Reminder
Configure `Amadeus` values (`BaseUrl`, `ClientId`, `ClientSecret`) in environment/app settings before running live provider flows.

## Provider Notes
Flights and locations use Amadeus. Hotels and activities remain mocked in Phase 3.
