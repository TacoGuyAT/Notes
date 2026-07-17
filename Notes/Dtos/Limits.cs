namespace Notes.Dtos;

public static class Limits {
    public const int Username = 64;
    public const int Password = 256;    // PBKDF2 input; no reason for a passphrase to exceed this
    public const int Title = 200;
    public const int Content = 200_000; // ~200 KB of markdown, re-rendered on every save
}
