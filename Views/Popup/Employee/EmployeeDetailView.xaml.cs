using BlockHouse.ViewModels.Popup.Employee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.Employee
{
    /// <summary>
    /// Interaction logic for EmployeeDetailView.xaml
    /// </summary>
    public partial class EmployeeDetailView : Window
    {
        public EmployeeDetailView()
        {
            InitializeComponent();
            DataContextChanged += EmployeeDetailView_DataContextChanged;
        }

        private void EmployeeDetailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is EmployeeDetailViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is EmployeeDetailViewModel newVm)
            {
                newVm.RequestClose += Close;
            }
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
