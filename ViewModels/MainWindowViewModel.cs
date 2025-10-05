// MainWindowViewModel.cs
using BlockHouse.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace BlockHouse.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;
        private string _activeButton;

        public MainWindowViewModel()
        {
            // Initialize command
            NavigateCommand = new RelayCommand<string>(Navigate);

            WeakReferenceMessenger.Default.Register<ChangeViewMessage>(this, (r, m) =>
            {
                Navigate(m.Destination);
            });

            WeakReferenceMessenger.Default.Register<ToggleOverlayMessage>(this, (r, m) =>
            {

                IsOverlayVisible = m.IsVisible;
            });
            Navigate("Overview");
        }
        public class ToggleOverlayMessage
        {
            public bool IsVisible { get; set; }
        }

        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set
            {
                _isOverlayVisible = value;
                OnPropertyChanged(nameof(IsOverlayVisible));
            }
        }

        // Add this message registration in constructor


        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public string ActiveButton
        {
            get => _activeButton;
            set
            {
                _activeButton = value;
                OnPropertyChanged(nameof(ActiveButton));
            }
        }

        private bool _backendStartedSuccessfully;

        public ICommand NavigateCommand { get; }

        public void Navigate(string destination)
        {
            switch (destination)
            {
                case "Overview":
                    if (ActiveButton != "Overview")
                    {
                        CurrentView = new Dashboard();
                    }
                    break;
                case "Revenue":
                    CurrentView = new RevenueView();
                    break;
                case "Employees":
                    CurrentView = new EmployeesView();
                    break;
                case "Services":
                    CurrentView = new ServicesView();
                    break;
            }
            ActiveButton = destination;
        }
    }
}