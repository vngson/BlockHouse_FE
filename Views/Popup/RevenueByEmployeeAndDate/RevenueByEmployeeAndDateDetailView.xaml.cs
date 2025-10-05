using BlockHouse.ViewModels.Popup.RevenueByEmployeeAndDate;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByEmployeeAndDate
{
    /// <summary>
    /// Interaction logic for RevenueByEmployeeAndDateDetailView.xaml
    /// </summary>
    public partial class RevenueByEmployeeAndDateDetailView : Window
    {
        public RevenueByEmployeeAndDateDetailView()
        {
            InitializeComponent();
            this.Loaded += RevenueByDateDetailView_Loaded;
        }

        private void RevenueByDateDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RevenueByEmployeeAndDateDetailViewModel viewModel)
            {
                viewModel.RequestClose += () => this.Close();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Bắt đầu kéo cửa sổ
                this.DragMove();
            }
        }
    }
}
