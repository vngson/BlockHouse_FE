using BlockHouse.ViewModels.Popup.RevenueByEmployee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.RevenueByEmployee
{
    /// <summary>
    /// Interaction logic for RevenueAddView.xaml
    /// </summary>
    public partial class RevenueAddView : Window
    {
        public RevenueAddView()
        {
            InitializeComponent();
            DataContextChanged += RevenueAddView_DataContextChanged;
            Loaded += async (s, e) =>
            {
                if (DataContext is RevenueByEmployeeAddViewModel vm)
                {
                    await vm.LoadDataAsync();
                }
            };
        }

        private void RevenueAddView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RevenueByEmployeeAddViewModel oldVm)
            {
                oldVm.RequestClose -= CloseWindow;
            }

            if (e.NewValue is RevenueByEmployeeAddViewModel newVm)
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
