using BlockHouse.ViewModels.Popup.RevenueByEmployee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByEmployee
{
    /// <summary>
    /// Interaction logic for RevenueByEmployeeDeleteView.xaml
    /// </summary>
    public partial class RevenueByEmployeeDeleteView : Window
    {
        public RevenueByEmployeeDeleteView()
        {
            InitializeComponent();

            // Subscribe to DataContext changed event
            DataContextChanged += RevenueByEmployeeDeleteView_DataContextChanged;
        }

        private void RevenueByEmployeeDeleteView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RevenueByEmployeeDeleteViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is RevenueByEmployeeDeleteViewModel newVm)
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
