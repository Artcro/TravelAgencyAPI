using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public sealed class TravelTicketSearchRequestValidator
{
    public IReadOnlyList<string> Validate(TravelTicketSearchRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Origin)) errors.Add("Origin is required.");
        if (string.IsNullOrWhiteSpace(request.Destination)) errors.Add("Destination is required.");
        if (request.Origin.Equals(request.Destination, StringComparison.OrdinalIgnoreCase)) errors.Add("Origin cannot equal destination.");
        if (request.DepartureDate < DateOnly.FromDateTime(DateTime.UtcNow.Date)) errors.Add("DepartureDate cannot be in the past.");
        if (request.ReturnDate is not null && request.ReturnDate <= request.DepartureDate) errors.Add("ReturnDate must be after DepartureDate.");
        if (request.Adults < 1) errors.Add("Adults must be >= 1.");
        if (request.Children < 0) errors.Add("Children must be >= 0.");
        if (request.Infants < 0) errors.Add("Infants must be >= 0.");
        if (request.MaxResults < 1 || request.MaxResults > 50) errors.Add("MaxResults must be between 1 and 50.");
        return errors;
    }
}
