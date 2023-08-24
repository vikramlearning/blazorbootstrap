namespace BlazorBootstrap;

/// <summary>
/// An interface that generates unique IDs.
/// </summary>
public interface IIdGenerator
{
    #region Methods

    /// <summary>
    /// Gets the next unique ID.
    /// </summary>
    string GetNextId();

    #endregion
}

/// <summary>
/// Generates efficient base32-encoded IDs.
/// <see href="https://github.com/aspnet/KestrelHttpServer/blob/6fde01a825cffc09998d3f8a49464f7fbe40f9c4/src/Kestrel.Core/Internal/Infrastructure/CorrelationIdGenerator.cs" />
/// </summary>
public sealed class IdGenerator : IIdGenerator
{
    #region Fields and Constants

    /// <summary>
    /// The length of the ID in characters.
    /// </summary>
    private const int IdLength = 13;

    /// <summary>
    /// The last generated ID.
    /// </summary>
    private static long LastId = DateTime.UtcNow.Ticks;

    /// <summary>
    /// The delegate used to generate the ID.
    /// </summary>
    private static readonly SpanAction<char, long> GenerateImplDelegate = GenerateImpl;

    #endregion

    #region Methods

    /// <summary>
    /// Generates a base32-encoded ID.
    /// </summary>
    /// <returns>The base32-encoded ID.</returns>
    public string GetNextId()
    {
        // Generate a new ID.
        var id = Interlocked.Increment(ref LastId);

        return string.Create(IdLength, id, GenerateImplDelegate);
    }

    /// <summary>
    /// Generates a base32-encoded ID using a specific algorithm.
    /// <seealso cref="https://stackoverflow.com/a/37271406" />
    /// </summary>
    /// <param name="buffer">The buffer to write the ID to.</param>
    /// <param name="id">The ID to generate.</param>
    private static void GenerateImpl(Span<char> buffer, long id)
    {
        const string encode32Chars = "ABCDEFGHIJKLMNOPQRSTUV0123456789";

        // Accessing the last item in the beginning elides range checks for all the subsequent items.
        buffer[12] = encode32Chars[(int)id & 31];
        buffer[0] = encode32Chars[(int)(id >> 60) & 31];
        buffer[1] = encode32Chars[(int)(id >> 55) & 31];
        buffer[2] = encode32Chars[(int)(id >> 50) & 31];
        buffer[3] = encode32Chars[(int)(id >> 45) & 31];
        buffer[4] = encode32Chars[(int)(id >> 40) & 31];
        buffer[5] = encode32Chars[(int)(id >> 35) & 31];
        buffer[6] = encode32Chars[(int)(id >> 30) & 31];
        buffer[7] = encode32Chars[(int)(id >> 25) & 31];
        buffer[8] = encode32Chars[(int)(id >> 20) & 31];
        buffer[9] = encode32Chars[(int)(id >> 15) & 31];
        buffer[10] = encode32Chars[(int)(id >> 10) & 31];
        buffer[11] = encode32Chars[(int)(id >> 5) & 31];
    }

    #endregion
}