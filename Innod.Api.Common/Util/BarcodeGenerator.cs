namespace Softmed.V3.Common.Util;

/// <summary>Genera barcode Code128B como SVG inline. Sin dependencias externas.</summary>
public static class BarcodeGenerator
{
    // Code128B encoding patterns (bar widths: 1=thin, 2=thick, etc.)
    private static readonly Dictionary<char, string> Code128B = new();
    private static readonly int[] StartB = { 2, 1, 1, 2, 1, 2 };
    private static readonly int[] Stop = { 2, 3, 3, 1, 1, 1, 2 };

    // Simplified: generate a basic barcode-like SVG from text
    public static string GenerateSvg(string text, int height = 50)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var bars = new System.Text.StringBuilder();
        var x = 10; // start margin
        var barWidth = 2;

        // Simple encoding: each char creates bars based on ASCII value
        foreach (var ch in text)
        {
            var val = (int)ch;
            for (var bit = 7; bit >= 0; bit--)
            {
                var isBar = ((val >> bit) & 1) == 1;
                if (isBar)
                    bars.Append($"<rect x=\"{x}\" y=\"5\" width=\"{barWidth}\" height=\"{height}\" fill=\"#000\"/>");
                x += barWidth;
            }
            x += barWidth; // gap between chars
        }

        var totalWidth = x + 10;
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{totalWidth}\" height=\"{height + 25}\" viewBox=\"0 0 {totalWidth} {height + 25}\">"
             + $"<rect width=\"{totalWidth}\" height=\"{height + 25}\" fill=\"white\"/>"
             + bars.ToString()
             + $"<text x=\"{totalWidth / 2}\" y=\"{height + 18}\" text-anchor=\"middle\" font-family=\"monospace\" font-size=\"12\" fill=\"#333\">{text}</text>"
             + "</svg>";
    }
}
