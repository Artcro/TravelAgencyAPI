using Microsoft.EntityFrameworkCore;
using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;
using TravelAgency.Domain.Trips;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;

namespace TravelAgency.Infrastructure.Travel;

public sealed class TravelPackageService(TravelDbContext db) : ITravelPackageService
{
	public async Task<Result<TravelPackageDto>> SaveAsync(Guid userId, SaveTravelPackageRequest request,
		CancellationToken cancellationToken)
	{
		var errors = new List<ValidationError>();
		if (string.IsNullOrWhiteSpace(request.Destino)) errors.Add(new ValidationError("destino", "destino is required"));
		if (string.IsNullOrWhiteSpace(request.DataViagem))
			errors.Add(new ValidationError("dataViagem", "dataViagem is required"));
		if (request.Viajantes < 1) errors.Add(new ValidationError("viajantes", "viajantes must be at least 1"));
		if (request.ValorVoo < 0) errors.Add(new ValidationError("valorVoo", "valorVoo must be >= 0"));
		if (errors.Count > 0) return Result<TravelPackageDto>.Invalid(errors);

		var total = request.ValorVoo + (request.HotelValor ?? 0m) + (request.CarroValor ?? 0m) +
		            (request.PasseioValor ?? 0m);

		var entity = new TravelPackageEntity
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Destino = request.Destino!.Trim(),
			Imagem = request.Imagem,
			DataViagem = request.DataViagem!,
			Viajantes = request.Viajantes,
			CiaAerea = request.CiaAerea,
			Hotel = NormalizeOptional(request.Hotel),
			HotelValor = request.HotelValor,
			Carro = NormalizeOptional(request.Carro),
			CarroValor = request.CarroValor,
			Passeio = NormalizeOptional(request.Passeio),
			PasseioValor = request.PasseioValor,
			ValorVoo = request.ValorVoo,
			ValorTotal = total,
			Status = PackageStatus.Confirmed,
			CreatedAtUtc = DateTime.UtcNow
		};

		db.TravelPackages.Add(entity);
		db.AuditLogs.Add(new AuditLogEntity
		{
			Id = Guid.NewGuid(), UserId = userId, Action = AuditAction.PackageSaved, ResourceType = "TravelPackage",
			ResourceId = entity.Id.ToString(), CreatedAtUtc = DateTime.UtcNow
		});

		await db.SaveChangesAsync(cancellationToken);
		return Result<TravelPackageDto>.Ok(ToDto(entity));
	}

	public async Task<IReadOnlyList<TravelPackageDto>> ListAsync(Guid userId, CancellationToken cancellationToken)
	{
		var entities = await db.TravelPackages.Where(x => x.UserId == userId && !x.IsDeleted)
			.OrderByDescending(x => x.CreatedAtUtc)
			.ToListAsync(cancellationToken);

		return entities.Select(ToDto).ToList();
	}

	public async Task<TravelPackageDto?> CancelAsync(Guid userId, Guid packageId, CancellationToken cancellationToken)
	{
		var entity = await db.TravelPackages
			.FirstOrDefaultAsync(x => x.Id == packageId && x.UserId == userId && !x.IsDeleted, cancellationToken);
		if (entity is null) return null;

		if (entity.Status != PackageStatus.Cancelled)
		{
			entity.Status = PackageStatus.Cancelled;
			entity.UpdatedAtUtc = DateTime.UtcNow;
			db.AuditLogs.Add(new AuditLogEntity
			{
				Id = Guid.NewGuid(), UserId = userId, Action = AuditAction.PackageCancelled,
				ResourceType = "TravelPackage", ResourceId = entity.Id.ToString(), CreatedAtUtc = DateTime.UtcNow
			});
			await db.SaveChangesAsync(cancellationToken);
		}

		return ToDto(entity);
	}

	public async Task<bool> DeleteAsync(Guid userId, Guid packageId, CancellationToken cancellationToken)
	{
		var entity = await db.TravelPackages
			.FirstOrDefaultAsync(x => x.Id == packageId && x.UserId == userId && !x.IsDeleted, cancellationToken);
		if (entity is null) return false;

		entity.IsDeleted = true;
		entity.UpdatedAtUtc = DateTime.UtcNow;
		await db.SaveChangesAsync(cancellationToken);
		return true;
	}

	private static string? NormalizeOptional(string? value)
	{
		if (string.IsNullOrWhiteSpace(value)) return null;
		var trimmed = value.Trim();
		return trimmed.Equals("nenhum", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
	}

	private static TravelPackageDto ToDto(TravelPackageEntity x)
	{
		return new TravelPackageDto
		{
			Id = x.Id,
			Destino = x.Destino,
			Imagem = x.Imagem,
			DataViagem = x.DataViagem,
			Viajantes = x.Viajantes,
			CiaAerea = x.CiaAerea,
			Hotel = x.Hotel,
			HotelValor = x.HotelValor,
			Carro = x.Carro,
			CarroValor = x.CarroValor,
			Passeio = x.Passeio,
			PasseioValor = x.PasseioValor,
			ValorVoo = x.ValorVoo,
			Valor = x.ValorTotal,
			Status = x.Status,
			CriadoEm = x.CreatedAtUtc
		};
	}
}
