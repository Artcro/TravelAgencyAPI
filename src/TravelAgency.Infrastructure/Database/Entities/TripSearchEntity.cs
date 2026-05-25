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
	public string Currency { get; set; } = Domain.ValueObjects.Currency.Default;

	/// <summary>
	/// Serialized <c>TripSearchRequest</c> using default
	/// <c>JsonSerializerOptions</c> (PascalCase). The wire-format equivalent is
	/// camelCase via <c>JsonPropertyName</c> attributes, so this stored JSON
	/// is internal-only and does not round-trip cleanly into the wire shape.
	/// </summary>
	public string RequestJson { get; set; } = "{}";

	/// <summary>Internal-only — see <see cref="RequestJson"/>.</summary>
	public string ResponseJson { get; set; } = "{}";
	public DateTime CreatedAtUtc { get; set; }
	public string ProviderStatus { get; set; } = string.Empty;
}