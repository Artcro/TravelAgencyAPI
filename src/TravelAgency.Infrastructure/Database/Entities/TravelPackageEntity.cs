using TravelAgency.Domain.Trips;

namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class TravelPackageEntity
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public ApplicationUser? User { get; set; }
	public string Destino { get; set; } = string.Empty;
	public string? Imagem { get; set; }
	public string DataViagem { get; set; } = string.Empty;
	public int Viajantes { get; set; }
	public string? CiaAerea { get; set; }
	public string? Hotel { get; set; }
	public decimal? HotelValor { get; set; }
	public string? Carro { get; set; }
	public decimal? CarroValor { get; set; }
	public string? Passeio { get; set; }
	public decimal? PasseioValor { get; set; }
	public decimal ValorVoo { get; set; }
	public decimal ValorTotal { get; set; }
	public string Status { get; set; } = PackageStatus.Confirmed;
	public bool IsDeleted { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? UpdatedAtUtc { get; set; }
}
