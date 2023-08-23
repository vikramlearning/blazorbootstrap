using System.Buffers;

namespace BlazorBootstrap.Utilities;

/// <summary>
/// Generates efficient base32-encoded ID.
/// <see
///     href="https://github.com/aspnet/KestrelHttpServer/blob/6fde01a825cffc09998d3f8a49464f7fbe40f9c4/src/Kestrel.Core/Internal/Infrastructure/CorrelationIdGenerator.cs" />
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
        var Encode32Chars = "ABCDEFGHIJKLMNOPQRSTUV0123456789";
        // Accessing the last item in the beginning elides range checks for all the subsequent items.
        var index = 12;

        do
        {
            buffer[index] = Encode32Chars[(int)id & 31];
            id >>= 5;
            index--;
        }
        while (id > 0 && index >= 0);
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