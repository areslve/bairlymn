// BairlyMN.Mvc/Infrastructure/CustomClaimsPrincipalFactory.cs
// Program.cs-д бүртгэх хэрэгтэй

using BairlyMN.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BairlyMN.Mvc.Infrastructure;

public class CustomClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public CustomClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Одоогийн claim-уудтай нийцүүлэх
        identity.AddClaim(new Claim("DisplayName", user.DisplayName));
        identity.AddClaim(new Claim("AvatarPath", user.AvatarPath ?? ""));
        identity.AddClaim(new Claim("IsVerified", user.IsVerified.ToString()));

        // ── Agent claim-ууд ─────────────────────────────────────
        identity.AddClaim(new Claim("IsAgent",
            user.HasActiveAgent.ToString()));

        if (user.AgentUntil.HasValue)
            identity.AddClaim(new Claim("AgentUntil",
                user.AgentUntil.Value.ToString("O")));

        return identity;
    }
}