namespace TravelAgency.Domain.ValueObjects;

/// <summary>
/// A non-negative monetary amount in an ISO-4217 currency. Mirrors
/// <c>MoneyDto</c> from the Application layer but enforces invariants —
/// <c>MoneyDto</c> stays as the wire shape.
/// </summary>
public readonly record struct Money
{
	public decimal Amount { get; }
	public string Currency { get; }

	public Money(decimal amount, string currency)
	{
		if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
		if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
			throw new ArgumentException("Currency must be a 3-letter ISO-4217 code.", nameof(currency));

		Amount = amount;
		Currency = currency.ToUpperInvariant();
	}
}
