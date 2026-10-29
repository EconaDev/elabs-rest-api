using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ELabs.RestApi.WebAPI.Middleware;

/// <summary>
/// Normalises client-supplied header values before they reach the audit and log tables.
/// Values are trimmed, stripped of control characters and truncated to the column length,
/// so an oversized header can never make an audit INSERT fail and turn a successful call into a 500.
/// </summary>
internal static partial class HeaderSanitizer
{
    // Column lengths in ELabs_REST_API_Audit / ELabs_REST_API_Logs.
    // Keep in sync with Database/Northwind_Tables.sql.
    public const int MaxIdentity = 200;   // UserEmail, UserName, FormName, FormSection
    public const int MaxUserAgent = 500;

    [GeneratedRegex(@"^[A-Za-z0-9\-_.]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex CorrelationIdPattern();

    /// <summary>
    /// Returns the value trimmed, without control characters, and truncated to
    /// <paramref name="maxLength"/>; <c>null</c> when nothing usable remains.
    /// Returns the original instance when no change is needed (no allocation).
    /// </summary>
    public static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        ReadOnlySpan<char> span = value.AsSpan().Trim();

        bool hasControl = false;
        foreach (char c in span)
        {
            if (char.IsControl(c))
            {
                hasControl = true;
                break;
            }
        }

        if (!hasControl && span.Length <= maxLength)
            return span.Length == value.Length ? value : span.ToString();

        // Slow path: strip control characters, then truncate.
        Span<char> buffer = span.Length <= 256 ? stackalloc char[span.Length] : new char[span.Length];
        int written = 0;
        foreach (char c in span)
        {
            if (!char.IsControl(c))
                buffer[written++] = c;
        }

        int length = Math.Min(written, maxLength);

        // Do not cut a surrogate pair in half.
        if (length > 0 && length < written && char.IsHighSurrogate(buffer[length - 1]))
            length--;

        ReadOnlySpan<char> result = buffer[..length].Trim();
        return result.IsEmpty ? null : result.ToString();
    }

    /// <summary>True when the value is 1-64 characters of A-Z a-z 0-9 - _ .</summary>
    public static bool IsValidCorrelationId([NotNullWhen(true)] string? value) =>
        value is not null && CorrelationIdPattern().IsMatch(value);
}
