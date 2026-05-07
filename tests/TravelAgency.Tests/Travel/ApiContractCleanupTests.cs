using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Api.Controllers;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Tests.Travel;

public class ApiContractCleanupTests
{
	[Fact]
	public void Frontend_Search_Request_Uses_Portuguese_Json_Property_Names()
	{
		var request = new FrontendTravelTicketSearchRequest();
		var json = JsonSerializer.Serialize(request);

		Assert.Contains("\"origem\"", json);
		Assert.Contains("\"destino\"", json);
		Assert.Contains("\"dataIda\"", json);
		Assert.Contains("\"dataVolta\"", json);
		Assert.Contains("\"adultos\"", json);
		Assert.Contains("\"criancas\"", json);
		Assert.Contains("\"bebes\"", json);
		Assert.Contains("\"moeda\"", json);
		Assert.Contains("\"classe\"", json);
		Assert.Contains("\"maxResultados\"", json);
	}

	[Fact]
	public void Frontend_Search_Request_Does_Not_Expose_English_Json_Property_Names()
	{
		var json = JsonSerializer.Serialize(new FrontendTravelTicketSearchRequest());

		Assert.DoesNotContain("\"origin\"", json);
		Assert.DoesNotContain("\"destination\"", json);
		Assert.DoesNotContain("\"departureDate\"", json);
		Assert.DoesNotContain("\"returnDate\"", json);
		Assert.DoesNotContain("\"adults\"", json);
		Assert.DoesNotContain("\"children\"", json);
		Assert.DoesNotContain("\"infants\"", json);
		Assert.DoesNotContain("\"currency\"", json);
		Assert.DoesNotContain("\"travelClass\"", json);
		Assert.DoesNotContain("\"maxResults\"", json);
	}

	[Fact]
	public void Frontend_Search_Request_Deserializes_Portuguese_Json_Names()
	{
		const string json = """
		                    {
		                      "origem": "GRU",
		                      "destino": "JFK",
		                      "dataIda": "2026-05-10",
		                      "dataVolta": "2026-05-20",
		                      "adultos": 1,
		                      "criancas": 0,
		                      "bebes": 0,
		                      "moeda": "BRL",
		                      "classe": "ECONOMY",
		                      "maxResultados": 10
		                    }
		                    """;

		var request = JsonSerializer.Deserialize<FrontendTravelTicketSearchRequest>(json);

		Assert.NotNull(request);
		Assert.Equal("GRU", request!.Origem);
		Assert.Equal("JFK", request.Destino);
		Assert.Equal(new DateOnly(2026, 5, 10), request.DataIda);
		Assert.Equal(new DateOnly(2026, 5, 20), request.DataVolta);
		Assert.Equal(1, request.Adultos);
		Assert.Equal("ECONOMY", request.Classe);
		Assert.Equal(10, request.MaxResultados);
	}

	[Fact]
	public void Frontend_Dto_Uses_Exact_Portuguese_Json_Property_Names()
	{
		var item = new FrontendTravelTicketDto();
		var json = JsonSerializer.Serialize(item);

		Assert.Contains("\"ciaAerea\"", json);
		Assert.Contains("\"horaPartidaIda\"", json);
		Assert.Contains("\"dataChegadaVolta\"", json);
		Assert.Contains("\"paradas\"", json);
		Assert.Contains("\"valor\"", json);
	}

	[Fact]
	public void Frontend_Dto_Does_Not_Expose_English_Ticket_Field_Names()
	{
		var json = JsonSerializer.Serialize(new FrontendTravelTicketDto());

		Assert.DoesNotContain("provider", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("providerOfferId", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("airlineCode", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("departureAirportCode", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("arrivalAirportCode", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("stops", json, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("price", json, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void Legacy_Travel_Tickets_Controller_File_Is_Removed()
	{
		var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
		Assert.False(File.Exists(Path.Combine(root, "src", "TravelAgency.Api", "Controllers",
			"TravelTicketsController.cs")));
	}

	[Fact]
	public void Trips_Controller_File_Still_Exists()
	{
		var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
		Assert.True(File.Exists(Path.Combine(root, "src", "TravelAgency.Api", "Controllers", "TripsController.cs")));
	}

	[Fact]
	public void Public_Contract_Routes_Do_Not_Use_Frontend_Prefix()
	{
		var ticketsRoute = typeof(FrontendTravelTicketsController)
			.GetCustomAttributes(typeof(RouteAttribute), false)
			.Cast<RouteAttribute>()
			.Single()
			.Template;

		var airportsRoute = typeof(AirportsController)
			.GetCustomAttributes(typeof(RouteAttribute), false)
			.Cast<RouteAttribute>()
			.Single()
			.Template;

		Assert.Equal("api/v1/travel-tickets", ticketsRoute);
		Assert.Equal("api/v1/airports", airportsRoute);
		Assert.DoesNotContain("/frontend", ticketsRoute, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("/frontend", airportsRoute, StringComparison.OrdinalIgnoreCase);
	}
}
