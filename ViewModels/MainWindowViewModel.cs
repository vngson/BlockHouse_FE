using BlockHouse.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace BlockHouse.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;
        private string _activeButton;

        private Dashboard _dashboardView;
        private RevenueView _revenueView;
        private EmployeesView _employeesView;
        private ServicesView _servicesView;

        public MainWindowViewModel()
        {
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

        public ICommand NavigateCommand { get; }

        public void Navigate(string destination)
        {
            switch (destination)
            {
                case "Overview":
                    if (ActiveButton != "Overview")
                    {
                        _dashboardView ??= new Dashboard();
                        CurrentView = _dashboardView;
                    }
                    break;
                case "Revenue":
                    _revenueView ??= new RevenueView();
                    CurrentView = _revenueView;
                    break;
                case "Employees":
                    _employeesView ??= new EmployeesView();
                    CurrentView = _employeesView;
                    break;
                case "Services":
                    _servicesView ??= new ServicesView();
                    CurrentView = _servicesView;
                    break;
            }
            ActiveButton = destination;
        }
    }
}
