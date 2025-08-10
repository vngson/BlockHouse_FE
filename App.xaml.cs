using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace BlockHouse
{
    public partial class App : Application
    {
        public App()
        {
            // Bắt lỗi UI thread
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Bắt lỗi non-UI thread
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText("ui_error.log", e.Exception.ToString());
            MessageBox.Show("Lỗi khởi động ứng dụng (UI). Xem ui_error.log");
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("app_error.log", e.ExceptionObject.ToString());
            MessageBox.Show("Lỗi không xác định. Xem app_error.log");
        }
    }
}
