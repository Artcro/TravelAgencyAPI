namespace TravelAgency.Application.Travel;

/// <summary>
/// Shared rules used by <see cref="TripSearchRequestValidator"/> and
/// <see cref="TravelTicketSearchRequestValidator"/>. Trips coming from either
/// endpoint feed the same Duffel provider, so the rules must agree.
/// </summary>
internal static class TripCriteriaValidator
{
	internal const int MinAdults = 1;
	internal const int MaxAdults = 9;
	internal const int MaxChildren = 9;
	internal const int MaxResultsUpperBound = 50;

	internal static void ValidateCommonCriteria(string origin, string destination, DateOnly departureDate,
		DateOnly? returnDate, int adults, int children, int infants, int maxResults, List<string> errors)
	{
		if (string.IsNullOrWhiteSpace(origin)) errors.Add("Origin is required.");
		if (string.IsNullOrWhiteSpace(destination)) errors.Add("Destination is required.");
		if (!string.IsNullOrWhiteSpace(origin) && !string.IsNullOrWhiteSpace(destination) &&
		    origin.Equals(destination, StringComparison.OrdinalIgnoreCase))
			errors.Add("Origin cannot equal destination.");

		if (departureDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
			errors.Add("DepartureDate cannot be in the past.");

		if (returnDate is not null && returnDate < departureDate)
			errors.Add("ReturnDate must be on or after DepartureDate.");

		if (adults < MinAdults || adults > MaxAdults)
			errors.Add($"Adults must be between {MinAdults} and {MaxAdults}.");

		if (children < 0 || children > MaxChildren) errors.Add($"Children must be between 0 and {MaxChildren}.");
		if (infants < 0) errors.Add("Infants must be >= 0.");
		if (maxResults < 1 || maxResults > MaxResultsUpperBound)
			errors.Add($"MaxResults must be between 1 and {MaxResultsUpperBound}.");
	}
}
