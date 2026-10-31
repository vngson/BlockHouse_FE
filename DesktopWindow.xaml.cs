using BlockHouse.Services;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;

namespace BlockHouse;

// A WPF shell with bundled local UI; it never loads the Vercel website.
public partial class DesktopWindow : Window
{
    private bool initializing;
    private bool ready;
    public DesktopWindow()
    {
        InitializeComponent();
        CaptionStateChanged(this, EventArgs.Empty);
        // WPF dimensions are DIPs. Keep restored bounds inside the work area at Windows DPI scaling.
        var workArea = SystemParameters.WorkArea;
        Width = Math.Min(1380, workArea.Width * 0.92);
        Height = Math.Min(900, workArea.Height * 0.92);
    }

    private async void InitializeDesktop(object sender, RoutedEventArgs args)
    {
        if (initializing || ready) return;
        initializing = true;
        RetryButton.Visibility = Visibility.Collapsed;
        try
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "web");
            if (!File.Exists(Path.Combine(folder, "index.html")))
                throw new InvalidOperationException("Thiếu giao diện offline. Hãy chạy build-desktop.ps1 hoặc cài lại BlockHouse Setup.");
            if (!await new BackendProcessService().StartBackendWithHealthCheck())
                throw new InvalidOperationException("Không kết nối được BE mới tại 127.0.0.1:5050. Kiểm tra BE cũ đang chiếm cổng hoặc xem log trong thư mục dữ liệu BlockHouse.");
            var profile = Path.Combine(BackendHost.DataDirectory, "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profile);
            await Browser.EnsureCoreWebView2Async(environment);
            var core = Browser.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreHostObjectsAllowed = false;
            core.Settings.IsWebMessageEnabled = true;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;
            var savedLogin = new RememberedLoginStore(Path.Combine(BackendHost.DataDirectory, "remembered-login.json"));
            core.WebMessageReceived += (_, message) =>
            {
                if (DesktopTheme.ReadMessage(message.Source, message.WebMessageAsJson) is bool dark) { ApplyCaptionTheme(dark); return; }
                var reply = savedLogin.HandleMessage(message.Source, message.WebMessageAsJson);
                if (reply != null) core.PostWebMessageAsJson(reply);
            };
            core.SetVirtualHostNameToFolderMapping("blockhouse.desktop", folder, CoreWebView2HostResourceAccessKind.Deny);
            core.NavigationStarting += (_, e) =>
            {
                if (!IsLocalUi(e.Uri) && !e.Uri.StartsWith("blob:https://blockhouse.desktop/", StringComparison.OrdinalIgnoreCase)) e.Cancel = true;
            };
            core.NewWindowRequested += (_, e) => e.Handled = true;
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += (_, e) =>
            {
                if (IsLocalUi(e.Request.Uri) || IsLocalApi(e.Request.Uri) || e.Request.Uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase) || e.Request.Uri.StartsWith("blob:https://blockhouse.desktop/", StringComparison.OrdinalIgnoreCase)) return;
                e.Response = environment.CreateWebResourceResponse(null, 403, "Blocked external request", "Content-Type: text/plain");
            };
            core.DownloadStarting += (_, e) =>
            {
                // CSV is generated as a blob by the authenticated API client.
                if (!e.DownloadOperation.Uri.StartsWith("blob:https://blockhouse.desktop/", StringComparison.OrdinalIgnoreCase)) e.Cancel = true;
            };
            core.ProcessFailed += (_, _) =>
            {
                Browser.Visibility = Visibility.Hidden;
                StartupPanel.Visibility = Visibility.Visible;
                StartupMessage.Text = "Bộ hiển thị gặp lỗi. Đóng và mở lại BlockHouse. Đơn đã thanh toán vẫn lưu trong BE; đơn đang nhập cần kiểm tra lại.";
            };
            var uiVersion = File.GetLastWriteTimeUtc(Path.Combine(folder, "index.html")).Ticks;
            core.Navigate($"https://blockhouse.desktop/index.html?v={uiVersion}#overview");
            Browser.Visibility = Visibility.Visible;
            StartupPanel.Visibility = Visibility.Collapsed;
            ready = true;
        }
        catch (WebView2RuntimeNotFoundException)
        {
            StartupMessage.Text = "Thiếu Microsoft Edge WebView2 Runtime. Chạy lại BlockHouse Setup để cài runtime offline.";
            RetryButton.Visibility = Visibility.Visible;
        }
        catch (Exception error)
        {
            BackendHost.LogStartup(error);
            StartupMessage.Text = error is InvalidOperationException ? error.Message :
                "Không khởi động được BlockHouse: " + error.Message + " (0x" + error.HResult.ToString("X8") + "). Xem log tại " + Path.Combine(BackendHost.DataDirectory, "logs", "launcher.log");
            RetryButton.Visibility = Visibility.Visible;
        }
        finally { initializing = false; }
    }

    private static bool IsLocalUi(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.Host == "blockhouse.desktop" && uri.Port == 443;
    private static bool IsLocalApi(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "http" && uri.Host == "127.0.0.1" && uri.Port == 5050 && uri.AbsolutePath.StartsWith("/api/web/", StringComparison.Ordinal);
    protected override void OnClosed(EventArgs e) { Browser.Dispose(); base.OnClosed(e); }
}
