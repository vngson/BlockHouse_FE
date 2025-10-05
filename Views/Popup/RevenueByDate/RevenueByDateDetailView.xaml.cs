using BlockHouse.ViewModels.Popup;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByDate
{
    /// <summary>
    /// Interaction logic for RevenueByDateDetailView.xaml
    /// </summary>
    public partial class RevenueByDateDetailView : Window
    {
        public RevenueByDateDetailView()
        {
            InitializeComponent();
            this.Loaded += RevenueByDateDetailView_Loaded;
        }

        private void RevenueByDateDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RevenueByDateDetailViewModel viewModel)
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
