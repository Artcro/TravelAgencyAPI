using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure;

public sealed class FrontendTravelTicketService(
    IFlightProvider flightProvider,
    TravelTicketSearchRequestValidator validator,
    IOptions<TravelSearchDefaultsOptions> defaultsOptions,
    ILogger<FrontendTravelTicketService> logger) : IFrontendTravelTicketService
{
    public async Task<IReadOnlyList<FrontendTravelTicketDto>> SearchAsync(FrontendTravelTicketSearchRequest request, CancellationToken cancellationToken)
    {
        request.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BRL" : request.Currency;
        request.TravelClass = string.IsNullOrWhiteSpace(request.TravelClass) ? "ECONOMY" : request.TravelClass;
        request.Origin = string.IsNullOrWhiteSpace(request.Origin)
            ? defaultsOptions.Value.DefaultOrigin ?? ""
            : request.Origin;

        var baseRequest = new TravelTicketSearchRequest
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
            MaxResults = request.MaxResults
        };

        var errors = validator.Validate(baseRequest);
        if (errors.Count > 0) throw new ArgumentException(string.Join(" ", errors));

        var tripRequest = new TripSearchRequest
        {
            Origin = baseRequest.Origin,
            Destination = baseRequest.Destination,
            DepartureDate = baseRequest.DepartureDate,
            ReturnDate = baseRequest.ReturnDate,
            Adults = baseRequest.Adults,
            Children = baseRequest.Children,
            Infants = baseRequest.Infants,
            Currency = baseRequest.Currency,
            TravelClass = baseRequest.TravelClass,
            MaxFlightResults = baseRequest.MaxResults,
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
            logger.LogWarning(ex, "Flight provider failed for frontend travel tickets search.");
            throw new InvalidOperationException("Flight provider failed.", ex);
        }

        var response = new List<FrontendTravelTicketDto>();
        foreach (var flight in flights)
        {
            if (flight.OutboundSegments is null || flight.OutboundSegments.Count == 0 || flight.TotalPrice is null)
            {
                continue;
            }

            var idaFirst = flight.OutboundSegments.FirstOrDefault();
            var idaLast = flight.OutboundSegments.LastOrDefault();
            if (idaFirst is null || idaLast is null)
            {
                continue;
            }

            var returnFirst = flight.ReturnSegments?.FirstOrDefault();
            var returnLast = flight.ReturnSegments?.LastOrDefault();

            response.Add(new FrontendTravelTicketDto
            {
                Id = response.Count + 1,
                CiaAerea = string.IsNullOrWhiteSpace(flight.AirlineName) ? flight.AirlineCode : flight.AirlineName,
                HoraPartidaIda = idaFirst.DepartureAt.ToString("HH:mm"),
                AeroPartidaIda = idaFirst.Origin,
                DataPartidaIda = idaFirst.DepartureAt.ToString("yyyy-MM-dd"),
                HoraChegadaIda = idaLast.ArrivalAt.ToString("HH:mm"),
                AeroChegadaIda = idaLast.Destination,
                DataChegadaIda = idaLast.ArrivalAt.ToString("yyyy-MM-dd"),
                HoraPartidaVolta = returnFirst?.DepartureAt.ToString("HH:mm"),
                AeroPartidaVolta = returnFirst?.Origin,
                DataPartidaVolta = returnFirst?.DepartureAt.ToString("yyyy-MM-dd"),
                HoraChegadaVolta = returnLast?.ArrivalAt.ToString("HH:mm"),
                AeroChegadaVolta = returnLast?.Destination,
                DataChegadaVolta = returnLast?.ArrivalAt.ToString("yyyy-MM-dd"),
                Paradas = flight.OutboundSegments.Count > 0 ? Math.Max(flight.OutboundSegments.Count - 1, 0) : Math.Max(flight.Stops, 0),
                Valor = flight.TotalPrice.Amount
            });
        }

        return response;
    }
}
