using System.Text.Json;

namespace AvaloniaKit.Tools.Extensions
{
    internal static class JsonExtensions
    {
        internal static string? TryGetStr(this JsonElement el, string prop)
        {
            if (!el.TryGetProperty(prop, out var v)) return null;
            if (v.ValueKind == JsonValueKind.String) return v.GetString();
            if (v.ValueKind == JsonValueKind.Null) return null;
            return v.ToString();
        }

        internal static long TryGetLong(this JsonElement el, string prop)
        {
            if (!el.TryGetProperty(prop, out var v)) return 0;
            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt64(out long val)) return val;
            if (v.ValueKind == JsonValueKind.String && long.TryParse(v.GetString(), out long sval)) return sval;
            return 0;
        }
    }
}
