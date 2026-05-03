using Microsoft.AspNetCore.Identity;

namespace TravelAgency.Infrastructure.Database.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
	public string DisplayName { get; set; } = string.Empty;
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? UpdatedAtUtc { get; set; }
	public bool IsActive { get; set; } = true;
}