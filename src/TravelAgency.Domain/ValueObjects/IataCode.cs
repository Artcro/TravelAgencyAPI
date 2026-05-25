namespace TravelAgency.Domain.ValueObjects;

/// <summary>
/// A 3-letter IATA airport/city code. Always uppercase, always exactly three
/// letters. Use <see cref="TryCreate"/> to parse user input.
/// </summary>
public readonly record struct IataCode(string Value)
{
	public static bool TryCreate(string? input, out IataCode code)
	{
		var normalized = input?.Trim().ToUpperInvariant();
		if (normalized is { Length: 3 } && normalized.All(char.IsLetter))
		{
			code = new IataCode(normalized);
			return true;
		}

		code = default;
		return false;
	}

	public override string ToString() => Value;
}
