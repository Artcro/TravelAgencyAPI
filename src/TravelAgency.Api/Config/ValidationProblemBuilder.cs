using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.Common;

namespace TravelAgency.Api.Config;

/// <summary>
/// Translates a list of <see cref="ValidationError"/> values into a
/// <see cref="ProblemDetails"/> body shaped as
/// <c>{ errors: [{ field, message }] }</c>.
/// </summary>
internal static class ValidationProblemBuilder
{
	public static ProblemDetails Build(IReadOnlyList<ValidationError> errors, string? instance = null)
	{
		var problem = new ProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Invalid request",
			Detail = "One or more validation errors occurred.",
			Instance = instance
		};

		problem.Extensions["errors"] = errors.Select(e => new { field = e.Field, message = e.Message }).ToArray();
		return problem;
	}
}
