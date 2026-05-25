using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Providers;

public interface IHotelProvider
{
	Task<IReadOnlyList<HotelOptionDto>> SearchHotelsAsync(TripSearchRequest request,
		CancellationToken cancellationToken);
}
