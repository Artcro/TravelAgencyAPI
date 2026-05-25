using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public sealed class TripSearchRequestValidator
{
	public IReadOnlyList<string> Validate(TripSearchRequest request)
	{
		var errors = new List<string>();
		TripCriteriaValidator.ValidateCommonCriteria(request.Origin, request.Destination, request.DepartureDate,
			request.ReturnDate, request.Adults, request.Children, request.Infants, request.MaxFlightResults, errors);

		return errors;
	}
}
