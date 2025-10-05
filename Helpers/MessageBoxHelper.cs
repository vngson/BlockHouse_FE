using BlockHouse.ViewModels.Components;
using System.Windows;

namespace BlockHouse.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowSafe(string message, List<string> buttons, MessageType type)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                // Đang trên UI thread - gọi trực tiếp
                CustomMessageBox.Show(message, buttons, type);
            }
            else
            {
                // Không trên UI thread - invoke
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    CustomMessageBox.Show(message, buttons, type);
                });
            }
        }

        public static async Task ShowSafeAsync(string message, List<string> buttons, MessageType type)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                CustomMessageBox.Show(message, buttons, type);
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CustomMessageBox.Show(message, buttons, type);
                });
            }
        }
    }
}
