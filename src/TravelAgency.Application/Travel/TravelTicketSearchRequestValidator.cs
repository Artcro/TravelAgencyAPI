using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public sealed class TravelTicketSearchRequestValidator
{
	public IReadOnlyList<string> Validate(TravelTicketSearchRequest request)
	{
		var errors = new List<string>();
		TripCriteriaValidator.ValidateCommonCriteria(request.Origin, request.Destination, request.DepartureDate,
			request.ReturnDate, request.Adults, request.Children, request.Infants, request.MaxResults, errors);

		return errors;
	}
}
