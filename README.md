# TravelAgencyApi

## Phase 2 status
Implemented travel search flow with provider abstractions, Amadeus-backed flights/locations, and mocked hotels/activities.

## Amadeus credentials
Required:
- `Amadeus:ClientId`
- `Amadeus:ClientSecret`
- `Amadeus:BaseUrl` (default `https://test.api.amadeus.com`)

### Configure with user-secrets
```bash
cd src/TravelAgency.Api
dotnet user-secrets set "Amadeus:ClientId" "your_client_id"
dotnet user-secrets set "Amadeus:ClientSecret" "your_client_secret"
```

### Configure with environment variables
```bash
export Amadeus__ClientId=your_client_id
export Amadeus__ClientSecret=your_client_secret
export Amadeus__BaseUrl=https://test.api.amadeus.com
```

## Example location request
```bash
curl "http://localhost:5000/api/v1/locations?query=rio"
```

## Example trip search
```bash
curl -X POST "http://localhost:5000/api/v1/trips/search" \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "GRU",
    "destination": "GIG",
    "departureDate": "2026-06-15",
    "returnDate": "2026-06-20",
    "adults": 1,
    "children": 0,
    "infants": 0,
    "currency": "BRL",
    "travelClass": "ECONOMY",
    "maxFlightResults": 10,
    "includeHotels": true,
    "includeActivities": true
  }'
```

> Hotels and activities are mocked providers in Phase 2.
