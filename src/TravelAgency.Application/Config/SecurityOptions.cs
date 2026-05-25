namespace TravelAgency.Application.Config;

public sealed class SecurityOptions
{
	public const string SectionName = "Security";
	public bool RequireAuthentication { get; set; }
}
