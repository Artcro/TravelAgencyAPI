# TravelAgencyApi

## Phase 4 Status
Phase 4 is implemented: a new open/anonymous flight card endpoint was added for frontend ticket UI (`POST /api/v1/travel-tickets/search`), with dedicated request validation, outbound-flight mapping, and tests.

## Phase 3 Status
Phase 3 is implemented: migrations setup instructions, saved-trip ownership hardening, service-level test expansion, Amadeus parsing hardening, rate limiting policies, and README operational guidance are in place.

## Travel Ticket Cards (Frontend Endpoint)
This endpoint exists specifically for frontend flight/ticket card display prototypes and simple listing.

- **Route:** `POST /api/v1/travel-tickets/search`
- **Auth:** open/anonymous for now (`[AllowAnonymous]`)
- **Purpose:** returns simplified flight/ticket cards from the existing flight provider flow
- **Scope:** no booking, reservation, payment, issuing, cancellation, or refunds

### Example Request
```json
{
  "origin": "RIO",
  "destination": "LIS",
  "departureDate": "2026-08-10",
  "returnDate": null,
  "adults": 1,
  "children": 0,
  "infants": 0,
  "currency": "BRL",
  "travelClass": "ECONOMY",
  "maxResults": 10
}
```

### Example Response
```json
{
  "items": [
    {
      "provider": "Amadeus",
      "providerOfferId": "1",
      "airlineCode": "TP",
      "airlineName": "TAP Air Portugal",
      "departureAirportCode": "GIG",
      "departureTime": "21:30",
      "departureAt": "2026-08-10T21:30:00",
      "arrivalAirportCode": "LIS",
      "arrivalTime": "11:10",
      "arrivalDate": "2026-08-11",
      "arrivalAt": "2026-08-11T11:10:00",
      "stops": 0,
      "price": {
        "amount": 4720.50,
        "currency": "BRL"
      }
    }
  ],
  "warnings": []
}
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
- `search-medium` for `/api/v1/trips/search` and `/api/v1/travel-tickets/search`
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
