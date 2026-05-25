using System.Text.Json.Serialization;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class AirportAutocompleteDto
{
	[JsonPropertyName("iata")]
	public string Iata { get; set; } = "";
	[JsonPropertyName("cidade")]
	public string Cidade { get; set; } = "";
	[JsonPropertyName("pais")]
	public string Pais { get; set; } = "";
	[JsonPropertyName("nome")]
	public string Nome { get; set; } = "";
}
