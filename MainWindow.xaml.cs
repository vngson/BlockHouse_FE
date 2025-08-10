using BlockHouse.ViewModels;
using System.Windows;

namespace BlockHouse
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}