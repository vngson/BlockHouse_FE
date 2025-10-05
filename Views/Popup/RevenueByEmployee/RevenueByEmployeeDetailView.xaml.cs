using BlockHouse.ViewModels.Popup.RevenueByEmployee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByEmployee
{
    /// <summary>
    /// Interaction logic for RevenueByEmployeeDetailWindow.xaml
    /// </summary>
    public partial class RevenueByEmployeeDetailView : Window
    {
        public RevenueByEmployeeDetailView()
        {
            InitializeComponent();

            // Subscribe to DataContext changed event
            DataContextChanged += RevenueByEmployeeDetailView_DataContextChanged;
        }

        private void RevenueByEmployeeDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RevenueByEmployeeDetailViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is RevenueByEmployeeDetailViewModel newVm)
            {
                newVm.RequestClose += Close;
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
