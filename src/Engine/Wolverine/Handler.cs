using System.Security.Claims;
using Engine.EFCore;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Http;

namespace Engine.Wolverine;

public abstract class Handler
{
    private const string SubjectClaimType = "sub";
    private const string PreferredUsernameClaimType = "preferred_username";
    private const string RolesClaimType = "roles";

    protected readonly IDbContext DbContext;

    protected Handler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    {
        DbContext = dbContextFactory.CreateDbContext();
        var user = contextAccessor.HttpContext?.User;
        Username = user?.Identity?.Name
                   ?? user?.FindFirstValue(PreferredUsernameClaimType)
                   ?? user?.FindFirstValue(ClaimTypes.Name);
        UserSubjectId = user?.FindFirstValue(SubjectClaimType);
        UserId = Guid.TryParse(UserSubjectId, out var userGuid) ? userGuid : null;
        Roles = ExtractRoles(user);
    }

    public string? Username { get; }
    public string? UserSubjectId { get; }
    public Guid? UserId { get; }
    public IReadOnlyCollection<string> Roles { get; }

    protected bool IsInRole(string roleName)
    {
        return Roles.Contains(roleName.ToLowerInvariant());
    }

    private static IReadOnlyCollection<string> ExtractRoles(ClaimsPrincipal? user)
    {
        if (user is null)
            return [];

        var identityRoleClaimTypes = user.Identities
            .Select(x => x.RoleClaimType)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        var roleClaims = user.Claims.Where(x =>
            identityRoleClaimTypes.Contains(x.Type, StringComparer.OrdinalIgnoreCase) ||
            x.Type.Equals(RolesClaimType, StringComparison.OrdinalIgnoreCase) ||
            x.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase));

        return roleClaims
            .Select(x => x.Value.ToLowerInvariant())
            .Distinct()
            .ToArray();
    }
}