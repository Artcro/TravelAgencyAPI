using System.Text.Json.Serialization;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class TravelPackageDto
{
	[JsonPropertyName("id")]
	public Guid Id { get; set; }
	[JsonPropertyName("destino")]
	public string Destino { get; set; } = "";
	[JsonPropertyName("imagem")]
	public string? Imagem { get; set; }
	[JsonPropertyName("dataViagem")]
	public string DataViagem { get; set; } = "";
	[JsonPropertyName("viajantes")]
	public int Viajantes { get; set; }
	[JsonPropertyName("ciaAerea")]
	public string? CiaAerea { get; set; }
	[JsonPropertyName("hotel")]
	public string? Hotel { get; set; }
	[JsonPropertyName("hotelValor")]
	public decimal? HotelValor { get; set; }
	[JsonPropertyName("carro")]
	public string? Carro { get; set; }
	[JsonPropertyName("carroValor")]
	public decimal? CarroValor { get; set; }
	[JsonPropertyName("passeio")]
	public string? Passeio { get; set; }
	[JsonPropertyName("passeioValor")]
	public decimal? PasseioValor { get; set; }
	[JsonPropertyName("valorVoo")]
	public decimal ValorVoo { get; set; }
	[JsonPropertyName("valor")]
	public decimal Valor { get; set; }
	[JsonPropertyName("status")]
	public string Status { get; set; } = "";
	[JsonPropertyName("criadoEm")]
	public DateTime CriadoEm { get; set; }
}
