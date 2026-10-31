using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;

namespace BlockHouse.Services;

public static class BackendHost
{
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockHouse", "data");
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(2) };

    public static async Task<bool> IsReady(CancellationToken token = default)
    {
        try
        {
            using var response = await Client.GetAsync("http://127.0.0.1:5050/api/web/health", token);
            if (!response.IsSuccessStatusCode) return false;
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
            return body.RootElement.GetProperty("app").GetString() == "BlockHouse" && body.RootElement.GetProperty("api_version").GetInt32() == 2;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch { return false; }
    }

    public static async Task<bool> IsResponding(CancellationToken token = default)
    {
        try { using var response = await Client.GetAsync("http://127.0.0.1:5050/api/web/health", token); return true; }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch { return false; }
    }

    public static void StartSupervisor()
    {
        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "BE.exe")))
            throw new InvalidOperationException("Thiếu BE.exe cạnh BlockHouse.exe. Rebuild project sau khi chuẩn bị backend bằng build-desktop.ps1.");
#if DEBUG
        // Do not keep a second BlockHouse.exe alive: it locks the assemblies Visual Studio rebuilds.
        var backend = Path.Combine(AppContext.BaseDirectory, "BE.exe");
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (!File.Exists(Path.Combine(folder.FullName, "BlockHouse.csproj"))) continue;
            var prepared = Path.GetFullPath(Path.Combine(folder.FullName, "..", "Blockhouse_BE", "dist", "BE.exe"));
            if (File.Exists(prepared)) backend = prepared;
            break;
        }
        Directory.CreateDirectory(DataDirectory);
        var start = new ProcessStartInfo(backend) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = DataDirectory };
        start.Environment["BLOCKHOUSE_DB_PATH"] = DeveloperDatabase();
        start.Environment["BLOCKHOUSE_ALLOWED_ORIGINS"] = Environment.GetEnvironmentVariable("BLOCKHOUSE_ALLOWED_ORIGINS") ?? "http://127.0.0.1:8080,http://localhost:8080";
        start.ArgumentList.Add("--port"); start.ArgumentList.Add("5050");
        Process.Start(start)?.Dispose();
#else
        var exe = Path.Combine(AppContext.BaseDirectory, "BlockHouse.exe");
        Process.Start(new ProcessStartInfo(exe, "--backend") { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = AppContext.BaseDirectory })?.Dispose();
#endif
    }

    // Runs on one dedicated thread, so the named mutex is released by its owner.
    public static void Run()
    {
        var sid = WindowsIdentity.GetCurrent().User?.Value ?? Environment.UserName;
        using var mutex = new Mutex(true, "Local\\BlockHouse-Backend-" + sid, out var owner);
        if (!owner) return;
        try
        {
            using var stop = new EventWaitHandle(false, EventResetMode.ManualReset, "Local\\BlockHouseBackendStop");
            stop.Reset();
            Directory.CreateDirectory(Path.Combine(DataDirectory, "logs"));
            var exe = Path.Combine(AppContext.BaseDirectory, "BE.exe");
            if (!File.Exists(exe)) { Log("BE.exe missing next to BlockHouse.exe."); return; }
            while (true)
            {
                if (stop.WaitOne(0)) return;
                if (IsReady().GetAwaiter().GetResult()) { if (stop.WaitOne(5000)) return; continue; }
                try
                {
                    var start = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = DataDirectory };
#if DEBUG
                    // F5 must use the developer's existing DB, not create a second shop in AppData.
                    start.Environment["BLOCKHOUSE_DB_PATH"] = DeveloperDatabase();
#else
                    start.ArgumentList.Add("--data-dir"); start.ArgumentList.Add(DataDirectory);
#endif
                    start.ArgumentList.Add("--port"); start.ArgumentList.Add("5050");
                    using var child = Process.Start(start);
                    if (child != null)
                    {
                        while (!child.WaitForExit(1000))
                        {
                            if (!stop.WaitOne(0)) continue;
                            if (!child.WaitForExit(30000)) { Log("BE shutdown timed out; forcing exit for update."); child.Kill(entireProcessTree: true); child.WaitForExit(); }
                            return;
                        }
                    }
                    Log("BE exited; retrying in 10 seconds.");
                }
                catch (Exception error) { Log("Cannot start BE: " + error.GetType().Name); }
                if (stop.WaitOne(10000)) return;
            }
        }
        finally { mutex.ReleaseMutex(); }
    }

    public static async Task<bool> Stop()
    {
        try { using var stop = EventWaitHandle.OpenExisting("Local\\BlockHouseBackendStop"); stop.Set(); }
        catch (WaitHandleCannotBeOpenedException) { return !await IsReady(); }
        for (var i = 0; i < 80; i++)
        {
            await Task.Delay(500);
            if (await IsReady()) continue;
            var sid = WindowsIdentity.GetCurrent().User?.Value ?? Environment.UserName;
            try
            {
                using var mutex = Mutex.OpenExisting("Local\\BlockHouse-Backend-" + sid);
                bool acquired;
                try { acquired = mutex.WaitOne(0); }
                catch (AbandonedMutexException) { acquired = true; }
                if (!acquired) continue;
                mutex.ReleaseMutex();
                return true;
            }
            catch (WaitHandleCannotBeOpenedException) { await Task.Delay(3000); return !await IsReady(); }
        }
        return false;
    }

#if DEBUG
    private static string DeveloperDatabase()
    {
        var configured = Environment.GetEnvironmentVariable("BLOCKHOUSE_DB_PATH");
        if (!string.IsNullOrEmpty(configured))
        {
            if (!Path.IsPathFullyQualified(configured) || !File.Exists(configured)) throw new InvalidOperationException("BLOCKHOUSE_DB_PATH phải trỏ tới database dev hiện có.");
            return configured;
        }
        const string legacy = @"D:\Release_BlackHouse\BlockHouse.db";
        if (File.Exists(legacy)) return legacy;
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
        {
            if (!File.Exists(Path.Combine(folder.FullName, "BlockHouse.csproj"))) continue;
            var database = Path.GetFullPath(Path.Combine(folder.FullName, "..", "Blockhouse_BE", "BlockHouse.db"));
            if (File.Exists(database)) return database;
            break;
        }
        throw new InvalidOperationException("Không tìm thấy DB dev. Chạy BE từ source hoặc đặt BLOCKHOUSE_DB_PATH trước khi F5.");
    }
#endif

    public static void LogStartup(Exception error) => Log("Desktop startup: " + error);

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(DataDirectory, "logs"));
            var file = Path.Combine(DataDirectory, "logs", "launcher.log");
            if (File.Exists(file) && new FileInfo(file).Length > 1_000_000) File.Move(file, file + ".previous", true);
            File.AppendAllText(file, DateTimeOffset.Now.ToString("O") + " " + message + Environment.NewLine);
        }
        catch { /* No credentials or shop data are written to launcher logs. */ }
    }
}
