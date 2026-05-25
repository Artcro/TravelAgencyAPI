namespace TravelAgency.Application.Common;

public readonly record struct ValidationError(string Field, string Message);

public sealed record Result<T>(T? Value, IReadOnlyList<ValidationError> Errors)
{
	public bool IsValid => Errors.Count == 0;

	public static Result<T> Ok(T value)
	{
		return new Result<T>(value, []);
	}

	public static Result<T> Invalid(params ValidationError[] errors)
	{
		return new Result<T>(default, errors);
	}

	public static Result<T> Invalid(IReadOnlyList<ValidationError> errors)
	{
		return new Result<T>(default, errors);
	}
}
