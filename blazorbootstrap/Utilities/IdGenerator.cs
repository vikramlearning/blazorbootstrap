namespace BlazorBootstrap.Utilities;

/// <summary>
/// An interface implemented by Id generators.
/// </summary>
public interface IIdGenerator
{
    #region Properties, Indexers

    /// <summary>
    /// Gets the newly generated and globally unique value.
    /// This value is guaranteed to be unique across all calls to the <see cref="Generate" /> property.
    /// </summary>
    string Generate { get; }

    #endregion
}

/// <summary>
/// Generates efficient base32-encoded ID.
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
    /// Generates a base32-encoded ID using a specific algorithm.
    /// <seealso cref="https://stackoverflow.com/a/37271406" />
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="id"></param>
    private static void GenerateImpl(Span<char> buffer, long id)
    {
        var encode32Chars = "ABCDEFGHIJKLMNOPQRSTUV0123456789";
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

    #region Interface Implementations

    /// <summary>
    /// Generates a base32-encoded ID.
    /// The ID is a 13-character string in base32 format. Example: <c>AR03U66PN4M6E</c>
    /// </summary>
    public string Generate
    {
        get
        {
            var id = Interlocked.Increment(ref LastId);

            return string.Create(IdLength, id, GenerateImplDelegate);
        }
    }

    #endregion
}