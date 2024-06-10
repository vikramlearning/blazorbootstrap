namespace BlazorBootstrap;

/// <summary>
/// Generates efficient base32-encoded IDs.
/// <see href="https://github.com/dotnet/aspnetcore/blob/main/src/Servers/Kestrel/shared/CorrelationIdGenerator.cs" />
/// <see href="https://github.com/dotnet/orleans/blob/main/src/Orleans.Core/Networking/Shared/CorrelationIdGenerator.cs" />
/// </summary>
public static class IdUtility
{
    #region Fields and Constants

    // Base32 encoding - in ascii sort order for easy text based sorting
    // Ref: https://stackoverflow.com/a/37271406
    private static readonly char[] encode32Chars = "ABCDEFGHIJKLMNOPQRSTUV0123456789".ToCharArray();

    /// <summary>
    /// The last generated ID.
    /// </summary>
    private static long lastId = DateTime.UtcNow.Ticks;

    #endregion

    #region Methods

    /// <summary>
    /// Generates a base32-encoded ID.
    /// </summary>
    /// <returns>The base32-encoded ID.</returns>
    public static string GetNextId() => GenerateId(Interlocked.Increment(ref lastId));

    private static string GenerateId(long id)
    {
        return string.Create(13, id, (buffer, value) =>
        {
            char[] encode32Chars = IdUtility.encode32Chars;

            buffer[12] = encode32Chars[value & 31];
            buffer[11] = encode32Chars[(value >> 5) & 31];
            buffer[10] = encode32Chars[(value >> 10) & 31];
            buffer[9] = encode32Chars[(value >> 15) & 31];
            buffer[8] = encode32Chars[(value >> 20) & 31];
            buffer[7] = encode32Chars[(value >> 25) & 31];
            buffer[6] = encode32Chars[(value >> 30) & 31];
            buffer[5] = encode32Chars[(value >> 35) & 31];
            buffer[4] = encode32Chars[(value >> 40) & 31];
            buffer[3] = encode32Chars[(value >> 45) & 31];
            buffer[2] = encode32Chars[(value >> 50) & 31];
            buffer[1] = encode32Chars[(value >> 55) & 31];
            buffer[0] = encode32Chars[(value >> 60) & 31];
        });
    }

    #endregion
}
