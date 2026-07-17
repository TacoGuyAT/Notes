using System.Security.Cryptography;

namespace Notes.Services;

public class PasswordHasher(string pepper) {
    private const int Iterations = 100_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public (string Hash, string Salt) Hash(string password) {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        return (Convert.ToBase64String(Derive(password, salt)), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt) {
        var expected = Convert.FromBase64String(hash);
        var actual = Derive(password, Convert.FromBase64String(salt));
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password + pepper, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
}
