using BlockHouse.ViewModels.Popup.RevenueByEmployee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByEmployee
{
    /// <summary>
    /// Interaction logic for RevenueUpdateView.xaml
    /// </summary>
    public partial class RevenueUpdateView : Window
    {
        public RevenueUpdateView(int orderId)
        {
            InitializeComponent();
            var viewModel = new RevenueByEmployeeUpdateViewModel();
            viewModel.SetOrderId(orderId);
            DataContext = viewModel;

            DataContextChanged += RevenueUpdateView_DataContextChanged;
            Loaded += async (s, e) =>
            {
                if (DataContext is RevenueByEmployeeUpdateViewModel vm)
                {
                    await vm.LoadDataAsync();
                }
            };
        }

        private void RevenueUpdateView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RevenueByEmployeeUpdateViewModel oldVm)
            {
                oldVm.RequestClose -= CloseWindow;
            }

            if (e.NewValue is RevenueByEmployeeUpdateViewModel newVm)
            {
                newVm.RequestClose += CloseWindow;
            }
        }

        private void CloseWindow()
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
