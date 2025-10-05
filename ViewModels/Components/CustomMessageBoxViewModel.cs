using BlockHouse.Views.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using static BlockHouse.ViewModels.MainWindowViewModel;

namespace BlockHouse.ViewModels.Components
{
    public enum MessageType
    {
        Information,
        Warning,
        Error,
        Question
    }

    public class MessageBoxButton
    {
        public string Text { get; set; }
        public Brush Color { get; set; }
    }

    public partial class CustomMessageBoxViewModel : ObservableObject
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public PackIconKind IconKind { get; set; }
        public Brush TitleBrush { get; set; }
        public ObservableCollection<MessageBoxButton> Buttons { get; set; }
        public ICommand ButtonClickCommand { get; set; }
        public string Result { get; set; }
        public Action CloseAction { get; set; }

        public CustomMessageBoxViewModel(string message, IEnumerable<string> buttons, MessageType messageType)
        {
            Message = message;
            Buttons = new ObservableCollection<MessageBoxButton>();

            foreach (var buttonText in buttons)
            {
                var button = new MessageBoxButton { Text = buttonText };

                switch (messageType)
                {
                    case MessageType.Information:
                        button.Color = Brushes.DodgerBlue;
                        break;
                    case MessageType.Warning:
                        button.Color = Brushes.Orange;
                        break;
                    case MessageType.Error:
                        button.Color = Brushes.Red;
                        break;
                    case MessageType.Question:
                        if (buttonText.ToLower() == "yes" || buttonText.ToLower() == "ok")
                            button.Color = Brushes.Green;
                        else if (buttonText.ToLower() == "no" || buttonText.ToLower() == "cancel")
                            button.Color = Brushes.Red;
                        else
                            button.Color = Brushes.Gray;
                        break;
                    default:
                        button.Color = Brushes.Gray;
                        break;
                }

                Buttons.Add(button);
            }


            switch (messageType)
            {
                case MessageType.Information:
                    Title = "Thông báo";
                    IconKind = PackIconKind.InformationOutline;
                    TitleBrush = Brushes.Blue;
                    break;
                case MessageType.Warning:
                    Title = "Cảnh báo";
                    IconKind = PackIconKind.WarningOutline;
                    TitleBrush = Brushes.Orange;
                    break;
                case MessageType.Error:
                    Title = "Lỗi";
                    IconKind = PackIconKind.ErrorOutline;
                    TitleBrush = Brushes.Red;
                    break;
                case MessageType.Question:
                    Title = "Xác nhận";
                    IconKind = PackIconKind.HelpOutline;
                    TitleBrush = Brushes.Green;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType));
            }

            ButtonClickCommand = new RelayCommand<string>(OnButtonClick);
        }

        private void OnButtonClick(string buttonText)
        {
            Result = buttonText;
            //WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            CloseAction?.Invoke();
        }
    }

    public static class CustomMessageBox
    {
        public static string Show(string message, IEnumerable<string> buttons, MessageType messageType)
        {
            WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = true });
            var vm = new CustomMessageBoxViewModel(message, buttons, messageType);
            var window = new CustomMessageBoxWindow { DataContext = vm };
            vm.CloseAction = () => window.Close();
            window.ShowDialog();
            return vm.Result;
        }
    }
}
