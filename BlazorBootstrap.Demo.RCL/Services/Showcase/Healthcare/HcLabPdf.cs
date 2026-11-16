using System.Globalization;
using System.Text;
using BlazorBootstrap.Demo.RCL.Models.Showcase.Healthcare;

namespace BlazorBootstrap.Demo.RCL.Services.Showcase.Healthcare;

/// <summary>
/// Writes a lab report as a small, single-page PDF (built-in Helvetica fonts, Windows-1252 text) from the tab's data, for
/// the <c>PdfViewer</c> and the download link. Nothing is fetched or sent; the document says it is a demo.
/// </summary>
public static class HcLabPdf
{
    #region Fields and Constants

    private const int Left = 56;

    private const int PageHeight = 792;

    private const int PageWidth = 612;

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>Returns the report as a base64 PDF (for a <c>data:application/pdf;base64,</c> URL).</summary>
    public static string ToBase64(HcLabOrder order, HcLabPanel panel, HcPatient patient, HcDoctor? doctor, HcClinic? clinic) =>
        Convert.ToBase64String(Build(order, panel, patient, doctor, clinic));

    /// <summary>Returns the report as PDF bytes.</summary>
    public static byte[] Build(HcLabOrder order, HcLabPanel panel, HcPatient patient, HcDoctor? doctor, HcClinic? clinic)
    {
        var content = new StringBuilder();
        var y = PageHeight - 64;

        void Text(string font, int size, int x, string text) =>
            content.Append(CultureInfo.InvariantCulture, $"BT /{font} {size} Tf {x} {y} Td ({Escape(text)}) Tj ET\n");

        void Line(int fromX, int toX) => content.Append(CultureInfo.InvariantCulture, $"{fromX} {y} m {toX} {y} l S\n");

        Text("F2", 18, Left, HcSeedData.ProductName + " - Lab report");
        y -= 16;
        Text("F1", 9, Left, "Demo document: the patient, doctor, and results are fictitious. Not medical advice.");
        y -= 26;

        Text("F2", 10, Left, "Patient");
        Text("F1", 10, Left + 110, $"{patient.Name}   (record {patient.Id.ToString(CultureInfo.InvariantCulture)})");
        y -= 15;
        Text("F2", 10, Left, "Date of birth");
        Text("F1", 10, Left + 110, patient.DateOfBirth is { } dob ? Date(dob) : "Not on file");
        y -= 15;
        Text("F2", 10, Left, "Ordered by");
        Text("F1", 10, Left + 110, $"{doctor?.Name ?? "Unknown doctor"}{(clinic is null ? null : $", {clinic.Name}")}");
        y -= 15;
        Text("F2", 10, Left, "Ordered");
        Text("F1", 10, Left + 110, Date(order.OrderedAt));
        y -= 15;
        Text("F2", 10, Left, "Released");
        Text("F1", 10, Left + 110, order.ReleasedAt is { } released ? Date(released) : "Not released");
        y -= 28;

        Text("F2", 13, Left, panel.Name);
        y -= 20;

        int[] columns = { Left, Left + 170, Left + 250, Left + 330, Left + 450 };
        Text("F2", 10, columns[0], "Test");
        Text("F2", 10, columns[1], "Result");
        Text("F2", 10, columns[2], "Unit");
        Text("F2", 10, columns[3], "Reference range");
        Text("F2", 10, columns[4], "Flag");
        y -= 6;
        Line(Left, PageWidth - Left);
        y -= 15;

        for (var index = 0; index < panel.Analytes.Count; index++)
        {
            var analyte = panel.Analytes[index];
            var value = index < order.Values.Count ? order.Values[index] : (double?)null;
            var flag = value is { } measured ? HcState.Flag(analyte, measured) : "Pending";

            Text("F1", 10, columns[0], analyte.Name);
            Text(flag is "High" or "Low" ? "F2" : "F1", 10, columns[1], value is { } shown ? Number(shown) : "-");
            Text("F1", 10, columns[2], analyte.Unit);
            Text("F1", 10, columns[3], $"{Number(analyte.Low)} - {Number(analyte.High)}");
            Text(flag is "High" or "Low" ? "F2" : "F1", 10, columns[4], flag);
            y -= 17;
        }

        y -= 4;
        Line(Left, PageWidth - Left);
        y -= 22;

        if (order.Comment.Length > 0)
        {
            Text("F2", 10, Left, "Comment from your doctor");
            y -= 15;

            foreach (var line in Wrap(order.Comment, 95).Take(20))
            {
                Text("F1", 10, Left, line);
                y -= 14;
            }
        }

        return Assemble(content.ToString());
    }

    #endregion

    #region Private Methods

    private static byte[] Assemble(string content)
    {
        var stream = Encode(content);
        var objects = new List<byte[]>
        {
            Encode("<< /Type /Catalog /Pages 2 0 R >>"),
            Encode("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            Encode(FormattableString.Invariant($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth} {PageHeight}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>")),
            Encode("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            Encode("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"),
            Concat(Encode(FormattableString.Invariant($"<< /Length {stream.Length} >>\nstream\n")), stream, Encode("\nendstream")),
        };

        using var output = new MemoryStream();
        var offsets = new List<long>();

        Write(output, Encode("%PDF-1.4\n"));

        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(output.Position);
            Write(output, Encode(FormattableString.Invariant($"{index + 1} 0 obj\n")));
            Write(output, objects[index]);
            Write(output, Encode("\nendobj\n"));
        }

        var xref = output.Position;
        var table = new StringBuilder(FormattableString.Invariant($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n"));

        foreach (var offset in offsets)
            table.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");

        table.Append(CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        Write(output, Encode(table.ToString()));

        return output.ToArray();
    }

    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(part => part).ToArray();

    private static string Date(DateTime date) => date.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    /// <summary>Encodes text as Windows-1252 (the PDF's WinAnsiEncoding); characters it can't show become "?".</summary>
    private static byte[] Encode(string text)
    {
        var bytes = new byte[text.Length];

        for (var index = 0; index < text.Length; index++)
        {
            bytes[index] = text[index] switch
            {
                '–' => 0x96,
                '—' => 0x97,
                '‘' => 0x91,
                '’' => 0x92,
                '“' => 0x93,
                '”' => 0x94,
                '•' => 0x95,
                '…' => 0x85,
                var c when c < 0x80 || (c >= 0xA0 && c <= 0xFF) => (byte)c,
                _ => (byte)'?',
            };
        }

        return bytes;
    }

    /// <summary>Escapes PDF string delimiters, writes superscript nine as "^9", and drops line breaks.</summary>
    private static string Escape(string text) =>
        text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("(", "\\(", StringComparison.Ordinal).Replace(")", "\\)", StringComparison.Ordinal)
            .Replace("⁹", "^9", StringComparison.Ordinal).Replace('\r', ' ').Replace('\n', ' ');

    private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static IEnumerable<string> Wrap(string text, int width)
    {
        foreach (var paragraph in text.Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n'))
        {
            var line = new StringBuilder();

            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length > 0 && line.Length + 1 + word.Length > width)
                {
                    yield return line.ToString();
                    line.Clear();
                }

                line.Append(line.Length > 0 ? " " : string.Empty).Append(word.Length > width ? word[..width] : word);
            }

            yield return line.ToString();
        }
    }

    private static void Write(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);

    #endregion

    #endregion
}
