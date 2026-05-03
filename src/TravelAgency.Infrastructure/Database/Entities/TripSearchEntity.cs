namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class TripSearchEntity
{
	public Guid Id { get; set; }
	public Guid? UserId { get; set; }
	public string Origin { get; set; } = string.Empty;
	public string Destination { get; set; } = string.Empty;
	public DateOnly DepartureDate { get; set; }
	public DateOnly? ReturnDate { get; set; }
	public int Adults { get; set; }
	public int Children { get; set; }
	public int Infants { get; set; }
	public string Currency { get; set; } = "BRL";
	public string RequestJson { get; set; } = "{}";
	public string ResponseJson { get; set; } = "{}";
	public DateTime CreatedAtUtc { get; set; }
	public string ProviderStatus { get; set; } = string.Empty;
}