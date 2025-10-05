using BlockHouse.ViewModels.Popup.Service;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.Service
{
    public partial class ServiceAddView : Window
    {
        public ServiceAddView()
        {
            InitializeComponent();
            DataContextChanged += ServiceAddView_DataContextChanged;
        }

        private void ServiceAddView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ServiceAddViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is ServiceAddViewModel newVm)
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