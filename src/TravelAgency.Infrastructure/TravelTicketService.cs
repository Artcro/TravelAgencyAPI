using Microsoft.Extensions.Logging;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;

namespace TravelAgency.Infrastructure;

public sealed class TravelTicketService(TravelTicketSearchRequestValidator validator, IFlightProvider flightProvider, ILogger<TravelTicketService> logger) : ITravelTicketService
{
    public async Task<TravelTicketSearchResponse> SearchAsync(TravelTicketSearchRequest request, CancellationToken cancellationToken)
    {
        request.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BRL" : request.Currency;
        request.TravelClass = string.IsNullOrWhiteSpace(request.TravelClass) ? "ECONOMY" : request.TravelClass;

        var errors = validator.Validate(request);
        if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

        var tripRequest = new TripSearchRequest
        {
            Origin = request.Origin,
            Destination = request.Destination,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            Adults = request.Adults,
            Children = request.Children,
            Infants = request.Infants,
            Currency = request.Currency,
            TravelClass = request.TravelClass,
            MaxFlightResults = request.MaxResults,
            IncludeHotels = false,
            IncludeActivities = false
        };

        IReadOnlyList<FlightOptionDto> flights;
        try
        {
            flights = await flightProvider.SearchFlightsAsync(tripRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Flight provider failed for travel tickets search.");
            throw new InvalidOperationException("Flight provider failed.", ex);
        }

        var response = new TravelTicketSearchResponse();
        // TODO: add return-flight card fields if frontend later needs round-trip display details.
        foreach (var flight in flights)
        {
            if (flight.OutboundSegments is null || flight.OutboundSegments.Count == 0)
            {
                response.Warnings.Add($"Skipped offer '{flight.ProviderOfferId}' because outbound segments are missing.");
                continue;
            }

            var first = flight.OutboundSegments.First();
            var last = flight.OutboundSegments.Last();
            var stops = Math.Max(flight.OutboundSegments.Count - 1, 0);

            response.Items.Add(new TravelTicketOptionDto
            {
                Provider = flight.Provider,
                ProviderOfferId = flight.ProviderOfferId,
                AirlineCode = flight.AirlineCode,
                AirlineName = string.IsNullOrWhiteSpace(flight.AirlineName) ? flight.AirlineCode : flight.AirlineName,
                DepartureAirportCode = first.Origin,
                DepartureTime = first.DepartureAt.ToString("HH:mm"),
                DepartureAt = first.DepartureAt,
                ArrivalAirportCode = last.Destination,
                ArrivalTime = last.ArrivalAt.ToString("HH:mm"),
                ArrivalDate = last.ArrivalAt.ToString("yyyy-MM-dd"),
                ArrivalAt = last.ArrivalAt,
                Stops = stops,
                Price = flight.TotalPrice
            });
        }

        return response;
    }
}
