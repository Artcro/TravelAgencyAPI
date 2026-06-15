using System.Text.Json.Serialization;

namespace TravelAgency.Application.DTOs.Travel;

public sealed class TicketExtrasDto
{
	[JsonPropertyName("hoteis")]
	public List<HotelFrontendDto> Hoteis { get; set; } = [];
	[JsonPropertyName("passeios")]
	public List<PasseioFrontendDto> Passeios { get; set; } = [];
	[JsonPropertyName("carros")]
	public List<CarroFrontendDto> Carros { get; set; } = [];
}

public sealed class HotelFrontendDto
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";
	[JsonPropertyName("nome")]
	public string Nome { get; set; } = "";
	[JsonPropertyName("endereco")]
	public string? Endereco { get; set; }
	[JsonPropertyName("estrelas")]
	public int? Estrelas { get; set; }
	[JsonPropertyName("precoPorNoite")]
	public decimal PrecoPorNoite { get; set; }
	[JsonPropertyName("imagem")]
	public string? Imagem { get; set; }
}

public sealed class PasseioFrontendDto
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";
	[JsonPropertyName("titulo")]
	public string Titulo { get; set; } = "";
	[JsonPropertyName("descricao")]
	public string? Descricao { get; set; }
	[JsonPropertyName("preco")]
	public decimal Preco { get; set; }
	[JsonPropertyName("duracao")]
	public string? Duracao { get; set; }
	[JsonPropertyName("imagem")]
	public string? Imagem { get; set; }
}

public sealed class CarroFrontendDto
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";
	[JsonPropertyName("modelo")]
	public string Modelo { get; set; } = "";
	[JsonPropertyName("lugares")]
	public int Lugares { get; set; }
	[JsonPropertyName("cambio")]
	public string Cambio { get; set; } = "";
	[JsonPropertyName("quilometragem")]
	public string Quilometragem { get; set; } = "";
	[JsonPropertyName("precoPorDia")]
	public decimal PrecoPorDia { get; set; }
	[JsonPropertyName("foto")]
	public string? Foto { get; set; }
}
