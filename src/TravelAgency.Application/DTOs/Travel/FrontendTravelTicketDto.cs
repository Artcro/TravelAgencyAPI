using System.Text.Json.Serialization;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class FrontendTravelTicketDto
{
	[JsonPropertyName("id")]
	public int Id { get; set; }
	[JsonPropertyName("ciaAerea")]
	public string CiaAerea { get; set; } = "";
	[JsonPropertyName("horaPartidaIda")]
	public string HoraPartidaIda { get; set; } = "";
	[JsonPropertyName("aeroPartidaIda")]
	public string AeroPartidaIda { get; set; } = "";
	[JsonPropertyName("dataPartidaIda")]
	public string DataPartidaIda { get; set; } = "";
	[JsonPropertyName("horaChegadaIda")]
	public string HoraChegadaIda { get; set; } = "";
	[JsonPropertyName("aeroChegadaIda")]
	public string AeroChegadaIda { get; set; } = "";
	[JsonPropertyName("dataChegadaIda")]
	public string DataChegadaIda { get; set; } = "";
	[JsonPropertyName("horaPartidaVolta")]
	public string? HoraPartidaVolta { get; set; }
	[JsonPropertyName("aeroPartidaVolta")]
	public string? AeroPartidaVolta { get; set; }
	[JsonPropertyName("dataPartidaVolta")]
	public string? DataPartidaVolta { get; set; }
	[JsonPropertyName("horaChegadaVolta")]
	public string? HoraChegadaVolta { get; set; }
	[JsonPropertyName("aeroChegadaVolta")]
	public string? AeroChegadaVolta { get; set; }
	[JsonPropertyName("dataChegadaVolta")]
	public string? DataChegadaVolta { get; set; }
	[JsonPropertyName("paradas")]
	public int Paradas { get; set; }
	[JsonPropertyName("valor")]
	public decimal Valor { get; set; }
}
