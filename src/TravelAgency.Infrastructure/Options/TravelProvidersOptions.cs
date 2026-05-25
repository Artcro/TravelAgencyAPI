namespace TravelAgency.Infrastructure.Options;

public sealed class TravelProvidersOptions
{
	public const string SectionName = "TravelProviders";
	public string FlightProvider { get; set; } = "Duffel";
	public string LocationProvider { get; set; } = "Local";

	/// <summary>
	/// True when <see cref="LocationProvider"/> selects the local airport
	/// database (any of Local / Database / OurAirports). One canonical home for
	/// this decision so DI and the health controller can't drift apart.
	/// </summary>
	public bool IsLocalLocationProvider()
	{
		return string.Equals(LocationProvider, "Local", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(LocationProvider, "Database", StringComparison.OrdinalIgnoreCase) ||
		       string.Equals(LocationProvider, "OurAirports", StringComparison.OrdinalIgnoreCase);
	}
}
