using System.Text.Json;

namespace BlockHouse.Services;

public static class DesktopTheme
{
    public static bool? ReadMessage(string source, string message)
    {
        if (!RememberedLoginStore.IsTrustedSource(source) || message.Length > 256) return null;
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            if (root.GetProperty("channel").GetString() != "blockhouse-theme") return null;
            return root.GetProperty("theme").GetString() switch { "dark" => true, "light" => false, _ => null };
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException) { return null; }
    }
}
