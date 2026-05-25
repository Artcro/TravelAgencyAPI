namespace TravelAgency.Domain.ValueObjects;

/// <summary>
/// ISO-4217 currency code helper. Defaults to <see cref="Default"/> (BRL) when
/// callers omit the value. Replaces hardcoded "BRL" string literals scattered
/// across services.
/// </summary>
public static class Currency
{
	public const string Default = "BRL";

	public static string Normalize(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? Default : value.Trim().ToUpperInvariant();
	}
}
