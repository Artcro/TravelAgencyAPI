namespace TravelAgency.Infrastructure.Services.Airports;

public static class PortugueseCountryNames
{
	private static readonly IReadOnlyDictionary<string, string> Names =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["AE"] = "Emirados Arabes Unidos",
			["AR"] = "Argentina",
			["AT"] = "Austria",
			["AU"] = "Australia",
			["BE"] = "Belgica",
			["BR"] = "Brasil",
			["CA"] = "Canada",
			["CH"] = "Suica",
			["CL"] = "Chile",
			["CN"] = "China",
			["CO"] = "Colombia",
			["DE"] = "Alemanha",
			["DK"] = "Dinamarca",
			["EG"] = "Egito",
			["ES"] = "Espanha",
			["FI"] = "Finlandia",
			["FR"] = "Franca",
			["GB"] = "Reino Unido",
			["GR"] = "Grecia",
			["IE"] = "Irlanda",
			["IL"] = "Israel",
			["IN"] = "India",
			["IT"] = "Italia",
			["JP"] = "Japao",
			["KR"] = "Coreia do Sul",
			["MA"] = "Marrocos",
			["MX"] = "Mexico",
			["NL"] = "Paises Baixos",
			["NO"] = "Noruega",
			["NZ"] = "Nova Zelandia",
			["PE"] = "Peru",
			["PT"] = "Portugal",
			["PY"] = "Paraguai",
			["QA"] = "Catar",
			["SA"] = "Arabia Saudita",
			["SE"] = "Suecia",
			["SG"] = "Singapura",
			["TH"] = "Tailandia",
			["TR"] = "Turquia",
			["US"] = "Estados Unidos",
			["UY"] = "Uruguai",
			["ZA"] = "Africa do Sul"
		};

	public static string Resolve(string countryCode, string fallback)
	{
		return Names.TryGetValue(countryCode, out var name) ? name : fallback;
	}
}
