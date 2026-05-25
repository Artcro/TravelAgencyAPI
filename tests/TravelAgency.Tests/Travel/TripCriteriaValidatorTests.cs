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

	private static IReadOnlyList<string> Run(TripSearchRequest r)
	{
		return new TripSearchRequestValidator().Validate(r);
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
		Assert.Contains("Origin is required.", Run(r));
	}

	[Fact]
	public void Destination_required()
	{
		var r = Valid();
		r.Destination = "";
		Assert.Contains("Destination is required.", Run(r));
	}

	[Fact]
	public void Origin_cannot_equal_destination()
	{
		var r = Valid();
		r.Origin = "GRU";
		r.Destination = "GRU";
		Assert.Contains("Origin cannot equal destination.", Run(r));
	}

	[Fact]
	public void Departure_in_past_invalid()
	{
		var r = Valid();
		r.DepartureDate = Today.AddDays(-1);
		Assert.Contains("DepartureDate cannot be in the past.", Run(r));
	}

	[Fact]
	public void Return_before_departure_invalid()
	{
		var r = Valid();
		r.ReturnDate = r.DepartureDate.AddDays(-1);
		Assert.Contains("ReturnDate must be on or after DepartureDate.", Run(r));
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
		Assert.Contains(Run(r), e => e.StartsWith("Adults"));
	}

	[Fact]
	public void Adults_above_nine_invalid()
	{
		var r = Valid();
		r.Adults = 10;
		Assert.Contains(Run(r), e => e.StartsWith("Adults"));
	}

	[Fact]
	public void Children_negative_invalid()
	{
		var r = Valid();
		r.Children = -1;
		Assert.Contains(Run(r), e => e.StartsWith("Children"));
	}

	[Fact]
	public void Children_above_nine_invalid()
	{
		var r = Valid();
		r.Children = 10;
		Assert.Contains(Run(r), e => e.StartsWith("Children"));
	}

	[Fact]
	public void Infants_negative_invalid()
	{
		var r = Valid();
		r.Infants = -1;
		Assert.Contains("Infants must be >= 0.", Run(r));
	}

	[Fact]
	public void MaxResults_zero_invalid()
	{
		var r = Valid();
		r.MaxFlightResults = 0;
		Assert.Contains(Run(r), e => e.StartsWith("MaxResults"));
	}

	[Fact]
	public void MaxResults_above_fifty_invalid()
	{
		var r = Valid();
		r.MaxFlightResults = 51;
		Assert.Contains(Run(r), e => e.StartsWith("MaxResults"));
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
		Assert.Contains("Origin cannot equal destination.", errors);
		Assert.Contains(errors, e => e.StartsWith("Adults"));
		Assert.Contains(errors, e => e.StartsWith("Children"));
		Assert.Contains(errors, e => e.StartsWith("MaxResults"));
	}
}
