namespace TravelAgency.Api.Options;

public sealed class SecurityOptions
{
	public const string SectionName = "Security";
	public bool RequireAuthentication { get; set; }
}