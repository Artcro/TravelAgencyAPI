using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TravelAgency.Application.DTOs.Travel;
using TravelAgency.Application.Travel;

namespace TravelAgency.Api.Controllers;

[ApiController, Route("api/v1/travel-tickets")]
public sealed class FrontendTravelTicketsController(IFrontendTravelTicketService frontendTravelTicketService)
	: ControllerBase
{
	[AllowAnonymous, EnableRateLimiting("search-medium"), HttpGet("search")]
	public async Task<ActionResult<IReadOnlyList<FrontendTravelTicketDto>>> SearchGet(
		[FromQuery] FrontendTravelTicketQueryRequest query, CancellationToken cancellationToken)
	{
		var validationError = TryBuildSearchRequest(query, out var request);
		if (validationError is not null) return BadRequest(new { error = validationError });

		var response = await frontendTravelTicketService.SearchAsync(request, cancellationToken);
		return Ok(response);
	}

	private static string? TryBuildSearchRequest(FrontendTravelTicketQueryRequest query,
		out FrontendTravelTicketSearchRequest request)
	{
		request = new FrontendTravelTicketSearchRequest();

		var origem = NormalizeIata(query.Origem);
		if (origem is null) return "origem is required/invalid";

		var destino = NormalizeIata(query.Destino);
		if (destino is null) return "destino is required/invalid";

		if (string.Equals(origem, destino, StringComparison.OrdinalIgnoreCase))
			return "origem and destino must differ";

		var departureValue = string.IsNullOrWhiteSpace(query.DataPartida) ? query.DataIda : query.DataPartida;
		if (!TryParseDate(departureValue, out var dataPartida)) return "dataPartida is required/invalid";
		if (dataPartida < DateOnly.FromDateTime(DateTime.UtcNow.Date)) return "dataPartida is required/invalid";

		DateOnly? dataVolta = null;
		if (!string.IsNullOrWhiteSpace(query.DataVolta))
		{
			if (!TryParseDate(query.DataVolta, out var parsedReturnDate)) return "dataVolta is required/invalid";
			if (parsedReturnDate < dataPartida) return "dataVolta is invalid";
			dataVolta = parsedReturnDate;
		}

		if (!int.TryParse(query.Adultos, NumberStyles.None, CultureInfo.InvariantCulture, out var adultos) ||
		    adultos is < 1 or > 9)
			return "adultos is required/invalid";

		var criancas = 0;
		if (!string.IsNullOrWhiteSpace(query.Criancas) &&
		    (!int.TryParse(query.Criancas, NumberStyles.None, CultureInfo.InvariantCulture, out criancas) ||
		     criancas is < 0 or > 9))
			return "criancas is required/invalid";

		var maxResultados = 10;
		if (!string.IsNullOrWhiteSpace(query.MaxResultados) &&
		    (!int.TryParse(query.MaxResultados, NumberStyles.None, CultureInfo.InvariantCulture, out maxResultados) ||
		     maxResultados is < 1 or > 50))
			return "maxResultados is required/invalid";

		request = new FrontendTravelTicketSearchRequest
		{
			Origem = origem,
			Destino = destino,
			DataIda = dataPartida,
			DataVolta = dataVolta,
			Adultos = adultos,
			Criancas = criancas,
			Bebes = 0,
			Moeda = "BRL",
			Classe = query.Classe ?? "ECONOMY",
			MaxResultados = maxResultados
		};

		return null;
	}

	[AllowAnonymous, EnableRateLimiting("search-medium"), HttpPost("search")]
	public async Task<ActionResult<IReadOnlyList<FrontendTravelTicketDto>>> Search(
		[FromBody] FrontendTravelTicketSearchRequest request, CancellationToken cancellationToken)
	{
		var response = await frontendTravelTicketService.SearchAsync(request, cancellationToken);
		return Ok(response);
	}

	private static string? NormalizeIata(string? value)
	{
		var normalized = value?.Trim().ToUpperInvariant();
		return normalized is { Length: 3 } && normalized.All(char.IsLetter) ? normalized : null;
	}

	private static bool TryParseDate(string? value, out DateOnly date)
	{
		return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
	}
}
