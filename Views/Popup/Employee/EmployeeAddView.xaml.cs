using BlockHouse.ViewModels.Popup.Employee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.Employee
{
    public partial class EmployeeAddView : Window
    {
        public EmployeeAddView()
        {
            InitializeComponent();
            DataContextChanged += EmployeeAddView_DataContextChanged;
        }

        private void EmployeeAddView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is EmployeeAddViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is EmployeeAddViewModel newVm)
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