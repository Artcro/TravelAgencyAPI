using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public sealed class TripResultNormalizer
{
    public TripSearchResponse Normalize(TripSearchRequest request, IReadOnlyList<FlightOptionDto> flights, IReadOnlyList<HotelOptionDto> hotels, IReadOnlyList<ActivityOptionDto> activities, List<string> warnings)
    {
        return new TripSearchResponse
        {
            Origin = new LocationSummaryDto(request.Origin, request.Origin),
            Destination = new LocationSummaryDto(request.Destination, request.Destination),
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BRL" : request.Currency,
            Flights = flights.ToList(), Hotels = hotels.ToList(), Activities = activities.ToList(), Warnings = warnings
        };
    }
}
