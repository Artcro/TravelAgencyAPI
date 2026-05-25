namespace TravelAgency.Domain.ValueObjects;

public enum TravelClass
{
	Economy,
	PremiumEconomy,
	Business,
	First
}

public static class TravelClassParser
{
	/// <summary>Wire-format token returned for the default cabin (Economy).</summary>
	public const string DefaultWireValue = "ECONOMY";

	public static TravelClass Parse(string? value)
	{
		return value?.Trim().ToUpperInvariant().Replace(" ", "_") switch
		{
			"PREMIUM_ECONOMY" or "PREMIUMECONOMY" => TravelClass.PremiumEconomy,
			"BUSINESS" => TravelClass.Business,
			"FIRST" => TravelClass.First,
			_ => TravelClass.Economy
		};
	}

	public static string ToWire(TravelClass value)
	{
		return value switch
		{
			TravelClass.PremiumEconomy => "PREMIUM_ECONOMY",
			TravelClass.Business => "BUSINESS",
			TravelClass.First => "FIRST",
			_ => "ECONOMY"
		};
	}
}
