using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public sealed class TravelTicketSearchRequestValidator
{
	public IReadOnlyList<ValidationError> Validate(TravelTicketSearchRequest request)
	{
		var errors = new List<ValidationError>();
		TripCriteriaValidator.ValidateCommonCriteria(request.Origin, request.Destination, request.DepartureDate,
			request.ReturnDate, request.Adults, request.Children, request.Infants, request.MaxResults, errors);

		return errors;
	}
}
