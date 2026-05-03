namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class ProviderRequestLogEntity
{
	public Guid Id { get; set; }
	public string Provider { get; set; } = string.Empty;
	public string Endpoint { get; set; } = string.Empty;
	public int? StatusCode { get; set; }
	public bool Success { get; set; }
	public string? ErrorMessage { get; set; }
	public long DurationMs { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}