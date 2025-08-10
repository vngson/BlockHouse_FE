// MainWindowViewModel.cs
using BlockHouse.Views;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Windows.Media;

namespace BlockHouse.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;
        private string _activeButton;

        public MainWindowViewModel()
        {
            // Set Dashboard as default view
            CurrentView = new Dashboard();
            ActiveButton = "Overview";

            // Initialize command
            NavigateCommand = new RelayCommand<string>(Navigate);
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

        private void Navigate(string destination)
        {
            ActiveButton = destination;

            switch (destination)
            {
                case "Overview":
                    CurrentView = new Dashboard();
                    break;
                case "Revenue":
                     CurrentView = new RevenueView();
                    break;
                case "Employees":
                     CurrentView = new EmployeesView();
                    break;
            }
        }
    }
}