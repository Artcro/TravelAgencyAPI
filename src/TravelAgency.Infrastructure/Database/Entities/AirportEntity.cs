namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class AirportEntity
{
	public string IataCode { get; set; } = "";
	public string? IcaoCode { get; set; }
	public string Ident { get; set; } = "";
	public string Name { get; set; } = "";
	public string? City { get; set; }
	public string CountryCode { get; set; } = "";
	public string CountryName { get; set; } = "";
	public string? CitySearch { get; set; }
	public string? NameSearch { get; set; }
	public string? CountrySearch { get; set; }
	public string AirportType { get; set; } = "";
	public bool ScheduledService { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime LastSyncedAtUtc { get; set; }
}
