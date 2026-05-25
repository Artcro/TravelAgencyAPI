using TravelAgency.Application.Common;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Tests.Travel;

/// <summary>
/// One test per rule on the shared trip-criteria validator. Future tweaks to
/// any single rule should fail exactly one of these tests so the drift is
/// visible.
/// </summary>
public class TripCriteriaValidatorTests
{
	private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
	private static readonly DateOnly Tomorrow = Today.AddDays(1);

	private static TripSearchRequest Valid()
	{
		return new TripSearchRequest
		{
			Origin = "GRU", Destination = "JFK", DepartureDate = Tomorrow, Adults = 1, Children = 0, Infants = 0,
			MaxFlightResults = 10
		};
	}

	private static IReadOnlyList<ValidationError> Run(TripSearchRequest r)
	{
		return new TripSearchRequestValidator().Validate(r);
	}

	private static bool HasField(IReadOnlyList<ValidationError> errors, string field)
	{
		return errors.Any(e => e.Field == field);
	}

	[Fact]
	public void Valid_request_has_no_errors()
	{
		Assert.Empty(Run(Valid()));
	}

	[Fact]
	public void Origin_required()
	{
		var r = Valid();
		r.Origin = "";
		Assert.True(HasField(Run(r), "origin"));
	}

	[Fact]
	public void Destination_required()
	{
		var r = Valid();
		r.Destination = "";
		Assert.True(HasField(Run(r), "destination"));
	}

	[Fact]
	public void Origin_cannot_equal_destination()
	{
		var r = Valid();
		r.Origin = "GRU";
		r.Destination = "GRU";
		Assert.Contains(Run(r), e => e.Message == "Origin cannot equal destination.");
	}

	[Fact]
	public void Departure_in_past_invalid()
	{
		var r = Valid();
		r.DepartureDate = Today.AddDays(-1);
		Assert.True(HasField(Run(r), "departureDate"));
	}

	[Fact]
	public void Return_before_departure_invalid()
	{
		var r = Valid();
		r.ReturnDate = r.DepartureDate.AddDays(-1);
		Assert.True(HasField(Run(r), "returnDate"));
	}

	[Fact]
	public void Return_same_day_as_departure_valid()
	{
		var r = Valid();
		r.ReturnDate = r.DepartureDate;
		Assert.Empty(Run(r));
	}

	[Fact]
	public void Adults_below_one_invalid()
	{
		var r = Valid();
		r.Adults = 0;
		Assert.True(HasField(Run(r), "adults"));
	}

	[Fact]
	public void Adults_above_nine_invalid()
	{
		var r = Valid();
		r.Adults = 10;
		Assert.True(HasField(Run(r), "adults"));
	}

	[Fact]
	public void Children_negative_invalid()
	{
		var r = Valid();
		r.Children = -1;
		Assert.True(HasField(Run(r), "children"));
	}

	[Fact]
	public void Children_above_nine_invalid()
	{
		var r = Valid();
		r.Children = 10;
		Assert.True(HasField(Run(r), "children"));
	}

	[Fact]
	public void Infants_negative_invalid()
	{
		var r = Valid();
		r.Infants = -1;
		Assert.True(HasField(Run(r), "infants"));
	}

	[Fact]
	public void MaxResults_zero_invalid()
	{
		var r = Valid();
		r.MaxFlightResults = 0;
		Assert.True(HasField(Run(r), "maxResults"));
	}

	[Fact]
	public void MaxResults_above_fifty_invalid()
	{
		var r = Valid();
		r.MaxFlightResults = 51;
		Assert.True(HasField(Run(r), "maxResults"));
	}

	[Fact]
	public void TravelTicket_validator_uses_same_rules()
	{
		var ticket = new TravelTicketSearchRequest
		{
			Origin = "GRU", Destination = "GRU", DepartureDate = Tomorrow, Adults = 10, Children = -1,
			MaxResults = 51
		};

		var errors = new TravelTicketSearchRequestValidator().Validate(ticket);
		Assert.Contains(errors, e => e.Message == "Origin cannot equal destination.");
		Assert.True(HasField(errors, "adults"));
		Assert.True(HasField(errors, "children"));
		Assert.True(HasField(errors, "maxResults"));
	}
}
