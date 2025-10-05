using BlockHouse.Helpers;
using BlockHouse.Services;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using static BlockHouse.ViewModels.MainWindowViewModel;

namespace BlockHouse
{
    public partial class App : Application
    {
        private BackendProcessService _backendService;
        private bool _backendStartedSuccessfully = false;

        public App()
        {
            // Handle exceptions in UI thread
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Handle exceptions in non-UI threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Handle task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AppLogger.Instance.LogError("UI thread exception", e.Exception);
            File.WriteAllText("ui_error.log", e.Exception.ToString());
            CustomMessageBox.Show(
                "Lỗi khởi động ứng dụng. Xem ui_error.log",
                new List<string> { "OK" },
                MessageType.Error
            );

            WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppLogger.Instance.LogError("Non-UI thread exception", e.ExceptionObject as Exception);
            File.WriteAllText("app_error.log", e.ExceptionObject.ToString());
            CustomMessageBox.Show(
                "Lỗi khởi động ứng dụng. Xem app_error.log",
                new List<string> { "OK" },
                MessageType.Error
            );
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AppLogger.Instance.LogError("Unobserved task exception", e.Exception);
            File.WriteAllText("task_error.log", e.Exception.ToString());

            CustomMessageBox.Show($"Task Error: {e.Exception.Message}", new List<string> { "OK" }, MessageType.Error);
            e.SetObserved();
        }
    }
}