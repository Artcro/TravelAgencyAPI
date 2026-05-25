namespace TravelAgency.Application.DTOs.Travel;

public sealed class FrontendTravelTicketQueryRequest
{
	public string? Origem { get; set; }
	public string? Destino { get; set; }
	public string? DataPartida { get; set; }
	public string? DataIda { get; set; }
	public string? DataVolta { get; set; }
	public string? Adultos { get; set; }
	public string? Criancas { get; set; }
	public string? Classe { get; set; }
	public string? MaxResultados { get; set; }
}
