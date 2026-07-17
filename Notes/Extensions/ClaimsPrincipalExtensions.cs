using System.Security.Claims;

namespace Notes.Extensions;

public static class ClaimsPrincipalExtensions {
    public const string UserIdClaim = "uid";
    public const string UserNameClaim = "name";
    public const string SecurityStampClaim = "stamp";

    /// <summary>Id of the signed-in user. Only call on endpoints that require authorization.</summary>
    public static long UserId(this ClaimsPrincipal user) => long.Parse(user.FindFirst(UserIdClaim)!.Value);

    public static string UserName(this ClaimsPrincipal user) => user.FindFirst(UserNameClaim)!.Value;
}
