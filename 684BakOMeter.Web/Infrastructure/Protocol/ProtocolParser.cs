using System.Globalization;

namespace _684BakOMeter.Web.Infrastructure.Protocol;

/// <summary>
/// Parses raw serial lines into <see cref="ProtocolMessage"/> instances
/// using the PREFIX:VALUE protocol format.
/// </summary>
public static class ProtocolParser
{
    /// <summary>
    /// Attempts to parse a raw serial line into a <see cref="ProtocolMessage"/>.
    /// Returns <c>false</c> for empty, whitespace-only, or malformed lines.
    /// </summary>
    public static bool TryParseLine(string line, out ProtocolMessage message)
    {
        message = default!;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        var colonIndex = line.IndexOf(':');
        if (colonIndex <= 0 || colonIndex == line.Length - 1)
            return false;

        var prefix = line[..colonIndex].Trim().ToUpperInvariant();
        var value = line[(colonIndex + 1)..].Trim();

        if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(value))
            return false;

        var type = prefix switch
        {
            "RFID" => ProtocolMessageType.Rfid,
            "SCALE1" or "SCALE2" => ProtocolMessageType.Scale,
            "ERROR" => ProtocolMessageType.Error,
            "SYSTEM" => ProtocolMessageType.System,
            _ => ProtocolMessageType.Unknown
        };

        message = new ProtocolMessage(type, prefix, value);
        return true;
    }

    /// <summary>
    /// Extracts the scale number from a SCALE prefix (e.g. "SCALE1" → 1).
    /// </summary>
    public static bool TryParseScaleNumber(string prefix, out int scaleNumber)
    {
        scaleNumber = 0;
        if (prefix.Length > 5
            && prefix.StartsWith("SCALE", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(prefix.AsSpan(5), out scaleNumber))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Parses a decimal measurement value using invariant culture.
    /// Supports formats like "123", "123.4", "123.45".
    /// </summary>
    public static bool TryParseDecimal(string raw, out decimal value)
    {
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
