using System.Security.Cryptography;

namespace Notes.Database;

public class User {
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Salt { get; set; } = "";

    /// <summary>
    /// Random per-account value, carried in the auth cookie and re-checked on every request.
    /// Ids are AUTOINCREMENT and can be reused by a later account after a delete; the stamp is what
    /// makes a cookie name one specific account rather than "whoever holds row N".
    /// </summary>
    public string SecurityStamp { get; set; } = "";

    public static string NewStamp() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
