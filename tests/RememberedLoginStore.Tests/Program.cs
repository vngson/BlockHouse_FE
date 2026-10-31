using BlockHouse.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

const string name = "temporary-test-owner";
const string password = "  temporary test password  ";
static void Check(bool condition, string label) { if (!condition) throw new Exception(label); }
if (args.Length == 2 && args[0] == "read")
{
    var restored = new RememberedLoginStore(args[1]).Load();
    Check(restored?.Name == name && restored.Password == password, "Restore in second process");
    return;
}
var directory = Directory.CreateTempSubdirectory("BlockHouse-login-test-").FullName;
try
{
    var path = Path.Combine(directory,"remembered-login.json");
    var store = new RememberedLoginStore(path);
    Check(store.Load() is null,"Missing file");
    store.Save(new(name,password));
    var encrypted = File.ReadAllText(path);
    Check(!encrypted.Contains(password) && !encrypted.Contains(name),"No plaintext credentials in JSON");
    Check(JsonDocument.Parse(encrypted).RootElement.GetProperty("version").GetInt32()==1,"JSON version");
    using (var child = Process.Start(new ProcessStartInfo(Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location,".exe")) { ArgumentList={"read",path}, UseShellExecute=false, CreateNoWindow=true })!)
    { child.WaitForExit(); Check(child.ExitCode==0,"Restore after process restart"); }
    var origin="https://blockhouse.desktop/index.html?v=123";
    Check(DesktopTheme.ReadMessage(origin,"{\"channel\":\"blockhouse-theme\",\"theme\":\"dark\"}")==true,"Dark caption theme");
    Check(DesktopTheme.ReadMessage(origin,"{\"channel\":\"blockhouse-theme\",\"theme\":\"light\"}")==false,"Light caption theme");
    Check(DesktopTheme.ReadMessage("https://evil.example/index.html","{\"channel\":\"blockhouse-theme\",\"theme\":\"dark\"}") is null,"Foreign caption theme blocked");
    Check(DesktopTheme.ReadMessage(origin,"{\"channel\":\"blockhouse-theme\",\"theme\":\"invalid\"}") is null,"Invalid caption theme blocked");
    Check(DesktopTheme.ReadMessage(origin,"not-json") is null,"Malformed caption message ignored");
    string Request(string action, object? account=null, string? target=null) => JsonSerializer.Serialize(new {channel="blockhouse-login",id=Guid.NewGuid().ToString(),action,@base=target ?? RememberedLoginStore.ApiBase,account});
    Check(store.HandleMessage("https://evil.example/index.html",Request("load")) is null,"Remote origin blocked");
    Check(store.HandleMessage("https://blockhouse.desktop.evil/index.html",Request("load")) is null,"Lookalike origin blocked");
    Check(store.HandleMessage("https://blockhouse.desktop/other.html",Request("load")) is null,"Other document blocked");
    var wrongBase=JsonDocument.Parse(store.HandleMessage(origin,Request("load",target:"http://127.0.0.1:5000"))!);
    Check(!wrongBase.RootElement.GetProperty("ok").GetBoolean(),"Wrong API base blocked");
    var response=JsonDocument.Parse(store.HandleMessage(origin,Request("load"))!);
    Check(response.RootElement.GetProperty("account").GetProperty("password").GetString()==password,"Trusted bridge reads JSON store");
    store.Save(new(name,"updated test password"));
    Check(new RememberedLoginStore(path).Load()?.Password=="updated test password","Password change replaces saved value");
    File.WriteAllText(path,"corrupt json");
    Check(store.Load() is null && File.ReadAllText(path)=="corrupt json","Corrupt file does not block login or get deleted");
    store.Forget(); store.Forget();
    Check(!File.Exists(path) && store.Load() is null,"Unchecked remember forgets saved account");
    Console.WriteLine("PASS native JSON store: encrypted at rest; restore across processes; origin/API guards; replacement; corrupt/missing file; forget.");
}
finally
{
    var resolved=Path.GetFullPath(directory);
    if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("BlockHouse-login-test-")) throw new Exception("Unexpected test cleanup path");
    Directory.Delete(resolved,true);
}
