using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Providers;
using TravelAgency.Application.Travel;
using TravelAgency.Domain.ValueObjects;

namespace TravelAgency.Infrastructure.Travel;

public sealed class TicketExtrasService(
	IHotelProvider hotelProvider,
	IActivityProvider activityProvider,
	ICarRentalProvider carRentalProvider) : ITicketExtrasService
{
	public async Task<Result<TicketExtrasDto>> GetExtrasAsync(string destino, string? moeda,
		CancellationToken cancellationToken)
	{
		if (!IataCode.TryCreate(destino, out var destinoCode))
			return Result<TicketExtrasDto>.Invalid(new ValidationError("destino", "destino is required/invalid"));

		var request = new TripSearchRequest
		{
			Destination = destinoCode.Value,
			Currency = Currency.Normalize(moeda)
		};

		var hotels = await hotelProvider.SearchHotelsAsync(request, cancellationToken);
		var activities = await activityProvider.SearchActivitiesAsync(request, cancellationToken);
		var cars = await carRentalProvider.SearchCarsAsync(request, cancellationToken);

		var dto = new TicketExtrasDto
		{
			Hoteis = hotels.Select(h => new HotelFrontendDto
			{
				Id = h.ProviderHotelId,
				Nome = h.Name,
				Endereco = h.Address ?? h.CityCode,
				Estrelas = h.Rating,
				PrecoPorNoite = h.PricePerNight?.Amount ?? 0m,
				Imagem = h.ImageUrl
			}).ToList(),
			Passeios = activities.Select(a => new PasseioFrontendDto
			{
				Id = a.ProviderActivityId,
				Titulo = a.Title,
				Descricao = a.Description,
				Preco = a.Price?.Amount ?? 0m,
				Duracao = a.Duration,
				Imagem = a.ImageUrl
			}).ToList(),
			Carros = cars.Select(c => new CarroFrontendDto
			{
				Id = c.ProviderCarId,
				Modelo = c.Model,
				Lugares = c.Seats,
				Cambio = c.Transmission,
				Quilometragem = c.Mileage,
				PrecoPorDia = c.PricePerDay?.Amount ?? 0m,
				Foto = c.ImageUrl
			}).ToList()
		};

		return Result<TicketExtrasDto>.Ok(dto);
	}
}
