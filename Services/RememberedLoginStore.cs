using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlockHouse.Services;

public sealed class RememberedLoginStore(string path)
{
    public const string ApiBase = "http://127.0.0.1:5050";
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("BlockHouse.Login.v1");
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public sealed record Account(string Name, string Password);
    private sealed record Envelope(int Version, string ApiBase, string ProtectedData);

    public static bool IsTrustedSource(string source) => Uri.TryCreate(source, UriKind.Absolute, out var uri)
        && uri.Scheme == "https" && uri.Host == "blockhouse.desktop" && uri.Port == 443 && uri.AbsolutePath == "/index.html";

    public Account? Load()
    {
        try
        {
            if (!File.Exists(path) || new FileInfo(path).Length > 16384) return null;
            var envelope = JsonSerializer.Deserialize<Envelope>(File.ReadAllText(path), Json);
            if (envelope is null || envelope.Version != 1 || envelope.ApiBase != ApiBase) return null;
            var clear = ProtectedData.Unprotect(Convert.FromBase64String(envelope.ProtectedData), Entropy, DataProtectionScope.CurrentUser);
            try
            {
                var account = JsonSerializer.Deserialize<Account>(clear, Json);
                return Valid(account) ? account : null;
            }
            finally { CryptographicOperations.ZeroMemory(clear); }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or CryptographicException or FormatException or ArgumentException)
        { return null; }
    }

    private static bool Valid(Account? account) => account is not null && !string.IsNullOrWhiteSpace(account.Name)
        && account.Name.Length <= 60 && account.Password is { Length: > 0 and <= 200 };

    public void Save(Account account)
    {
        if (!Valid(account)) throw new ArgumentException("Invalid saved login.");
        var clear = JsonSerializer.SerializeToUtf8Bytes(account, Json);
        byte[] encrypted;
        try { encrypted = ProtectedData.Protect(clear, Entropy, DataProtectionScope.CurrentUser); }
        finally { CryptographicOperations.ZeroMemory(clear); }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(new Envelope(1, ApiBase, Convert.ToBase64String(encrypted)), Json));
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public void Forget() { if (File.Exists(path)) File.Delete(path); }

    // Only the bundled top-level login page can access this one fixed encrypted file.
    public string? HandleMessage(string source, string message)
    {
        if (!IsTrustedSource(source) || message.Length > 8192) return null;
        string? id = null;
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || root.GetProperty("channel").GetString() != "blockhouse-login") return null;
            id = root.GetProperty("id").GetString();
            if (!Guid.TryParse(id, out _)) return null;
            if (root.GetProperty("base").GetString() != ApiBase) throw new ArgumentException("Unsupported API.");
            Account? account = null;
            switch (root.GetProperty("action").GetString())
            {
                case "load": account = Load(); break;
                case "save":
                    var input = root.GetProperty("account");
                    Save(new Account(input.GetProperty("name").GetString()!, input.GetProperty("password").GetString()!));
                    break;
                case "forget": Forget(); break;
                default: throw new ArgumentException("Unsupported login action.");
            }
            return JsonSerializer.Serialize(new {channel = "blockhouse-login", id, ok = true, account}, Json);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or CryptographicException or ArgumentException or InvalidOperationException or KeyNotFoundException)
        { return id is null ? null : JsonSerializer.Serialize(new {channel = "blockhouse-login", id, ok = false}, Json); }
    }
}
