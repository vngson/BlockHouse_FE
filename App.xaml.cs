using BlockHouse.Services;
using System.Windows;

namespace BlockHouse;

public partial class App : Application
{
    private Mutex? desktopMutex;
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (e.Args.Contains("--stop-backend"))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Shutdown(await BackendHost.Stop() ? 0 : 1);
            return;
        }
        if (e.Args.Contains("--backend"))
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            await Task.Run(BackendHost.Run);
            Shutdown();
            return;
        }
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        desktopMutex = new Mutex(true, "Local\\BlockHouseDesktop", out var ownsDesktop);
        if (!ownsDesktop) { MessageBox.Show("BlockHouse đang mở. Hãy dùng cửa sổ hiện tại.", "BlockHouse"); Shutdown(); return; }
        MainWindow = new DesktopWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e) { desktopMutex?.Dispose(); base.OnExit(e); }
}
