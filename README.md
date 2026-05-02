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
