using System.Globalization;
using System.Text;

namespace TravelAgency.Infrastructure.Services.Airports;

public static class AirportTextNormalizer
{
	public static string Normalize(string? value)
	{
		if (string.IsNullOrWhiteSpace(value)) return "";

		var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
		var builder = new StringBuilder(decomposed.Length);
		var previousWasWhiteSpace = false;

		foreach (var c in decomposed)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;

			if (char.IsWhiteSpace(c))
			{
				if (!previousWasWhiteSpace) builder.Append(' ');
				previousWasWhiteSpace = true;
				continue;
			}

			builder.Append(char.ToLowerInvariant(c));
			previousWasWhiteSpace = false;
		}

		return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
	}
}
