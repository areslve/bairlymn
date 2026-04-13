using BairlyMN.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BairlyMN.Mvc.Infrastructure;

/// <summary>
/// Нэвтрэх үед avatar, displayName зэргийг Claim болгон нэмнэ.
/// Ингэснээр хаа сайгүй UserManager дуудахгүйгээр ашиглах боломжтой.
/// </summary>
public class CustomClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public CustomClaimsFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaims(new[]
        {
            new Claim("AvatarPath",   user.AvatarPath   ?? string.Empty),
            new Claim("DisplayName",  user.DisplayName),
            new Claim("FirstName",    user.FirstName    ?? string.Empty),
            new Claim("IsVerified",   user.IsVerified.ToString())
        });

        return identity;
    }
}