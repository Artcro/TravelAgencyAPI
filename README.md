# TravelAgencyApi

## Phase 4 Status
Phase 4 is implemented with deployment/frontend contract polish: production Swagger toggle, startup migrations toggle, and a frontend-contract endpoint `POST /api/v1/frontend/travel-tickets/search`.

## Phase 3 Status
Phase 3 is implemented: migrations setup instructions, saved-trip ownership hardening, service-level test expansion, Amadeus parsing hardening, rate limiting policies, and README operational guidance are in place.

## Frontend Contract Endpoint (Open)
- **Route:** `POST /api/v1/frontend/travel-tickets/search`
- **Auth:** open/anonymous (`[AllowAnonymous]`)
- **Response shape:** raw JSON array (no wrapper object)


## Broader Trips Endpoint
- **Route:** `POST /api/v1/trips/search`
- **Purpose:** broader aggregate trip search including flights, mocked hotels, and mocked activities.
- **Note:** this is not the frontend ticket-card contract endpoint.

Production docs on Render:
- Swagger UI: https://travelagencyapi-a5zb.onrender.com/swagger
- OpenAPI JSON: https://travelagencyapi-a5zb.onrender.com/swagger/v1/swagger.json

### Example Request (origin + destination + dates)
```json
{
  "origin": "GRU",
  "destination": "JFK",
  "departureDate": "2026-05-10",
  "returnDate": "2026-05-20",
  "adults": 1,
  "children": 0,
  "infants": 0,
  "currency": "BRL",
  "travelClass": "ECONOMY",
  "maxResults": 10
}
```

### Example Request (destination + date only, using configured default origin)
```json
{
  "destination": "JFK",
  "departureDate": "2026-05-10",
  "returnDate": "2026-05-20"
}
```

### Example Response (frontend field names)
```json
[
  {
    "id": 1,
    "ciaAerea": "LATAM",
    "horaPartidaIda": "08:30",
    "aeroPartidaIda": "GRU",
    "dataPartidaIda": "2026-05-10",
    "horaChegadaIda": "12:00",
    "aeroChegadaIda": "JFK",
    "dataChegadaIda": "2026-05-10",
    "horaPartidaVolta": "15:00",
    "aeroPartidaVolta": "JFK",
    "dataPartidaVolta": "2026-05-20",
    "horaChegadaVolta": "22:00",
    "aeroChegadaVolta": "GRU",
    "dataChegadaVolta": "2026-05-20",
    "paradas": 0,
    "valor": 3500.0
  }
]
```

### Notes
- Outbound summary only for now.
- If `returnDate` is provided, total offer price is preserved, but return-card fields are intentionally not exposed yet.

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
- `search-medium` for `/api/v1/trips/search` and `/api/v1/frontend/travel-tickets/search`
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
Flights and locations use Amadeus. Hotels and activities remain mocked.

## Deploying to Render
1. In Render, create a **Web Service** and connect your GitHub repository.
2. Select the branch you want to deploy.
3. Choose **Docker** as the deployment method (Render will use the root `Dockerfile`).
4. Add required environment variables in the Render dashboard (do **not** commit secrets):
   - `ASPNETCORE_ENVIRONMENT`
   - `Security__RequireAuthentication`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__Secret`
   - `Jwt__AccessTokenMinutes`
   - `Jwt__RefreshTokenDays`
   - `Amadeus__BaseUrl`
   - `Amadeus__ClientId`
   - `Amadeus__ClientSecret`
   - `ConnectionStrings__DefaultConnection`
   - `Cors__AllowAnyOrigin`
   - `Cors__AllowedOrigins__0`
   - `Swagger__Enabled=true`
   - `Database__ApplyMigrationsOnStartup=true`
   - `TravelSearchDefaults__DefaultOrigin=GRU`
5. Create or connect a Render PostgreSQL instance, and use the internal database URL/connection string where possible for `ConnectionStrings__DefaultConnection`.
6. Run EF Core migrations either locally (against the target DB) or from a secure shell/one-off environment that can reach the Render database.
7. Set production Amadeus credentials in Render (`Amadeus__ClientId`, `Amadeus__ClientSecret`).
8. Set `Cors__AllowedOrigins__0` to your deployed frontend URL.

Use `.env.example` as a template for local/Render variable names only. Never commit real secrets or a real `.env` file.

### Render CORS Safety
- For local demo/development, `Cors__AllowAnyOrigin=true` is convenient.
- For production, set `Cors__AllowAnyOrigin=false` and explicitly configure allowed frontend domains with `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.
- When `Cors__AllowAnyOrigin=true`, the API uses `AllowAnyOrigin + AllowAnyHeader + AllowAnyMethod` and does not use credentials mode.


## Flight Provider Defaults (Duffel)
Amadeus Self-Service is now legacy/optional for new demos. Default flight provider is **Duffel** test mode and default location provider is **Mock** for easier deployment.

### Render environment variables
- `TravelProviders__FlightProvider=Duffel`
- `TravelProviders__LocationProvider=Mock`
- `Duffel__BaseUrl=https://api.duffel.com`
- `Duffel__AccessToken=<duffel_test_token>`
- `Duffel__Version=v2`

Optional legacy Amadeus settings (only when selecting `Amadeus` provider):
- `Amadeus__BaseUrl`
- `Amadeus__ClientId`
- `Amadeus__ClientSecret`

Hotels and activities remain mocked.
