using System.Text.Json.Serialization;
using TravelAgency.Domain.ValueObjects;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class FrontendTravelTicketSearchRequest
{
	[JsonPropertyName("origem")]
	public string? Origem { get; set; }
	[JsonPropertyName("destino")]
	public string Destino { get; set; } = "";
	[JsonPropertyName("dataIda")]
	public DateOnly DataIda { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
	[JsonPropertyName("dataVolta")]
	public DateOnly? DataVolta { get; set; }
	[JsonPropertyName("adultos")]
	public int Adultos { get; set; } = 1;
	[JsonPropertyName("criancas")]
	public int Criancas { get; set; }
	[JsonPropertyName("bebes")]
	public int Bebes { get; set; }
	[JsonPropertyName("moeda")]
	public string Moeda { get; set; } = Currency.Default;
	[JsonPropertyName("classe")]
	public string Classe { get; set; } = TravelClassParser.DefaultWireValue;
	[JsonPropertyName("maxResultados")]
	public int MaxResultados { get; set; } = 10;
}
