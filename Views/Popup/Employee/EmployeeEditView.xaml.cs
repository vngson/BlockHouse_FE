using BlockHouse.ViewModels.Popup.Employee;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.Employee
{
    public partial class EmployeeEditView : Window
    {
        public EmployeeEditView()
        {
            InitializeComponent();
            DataContextChanged += EmployeeEditView_DataContextChanged;
        }

        private void EmployeeEditView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is EmployeeEditViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is EmployeeEditViewModel newVm)
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