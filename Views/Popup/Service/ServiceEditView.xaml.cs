using BlockHouse.ViewModels.Popup.Service;
using System.Windows;
using System.Windows.Input;

namespace BlockHouse.Views.Popup.Service
{
    public partial class ServiceEditView : Window
    {
        public ServiceEditView()
        {
            InitializeComponent();
            DataContextChanged += ServiceEditView_DataContextChanged;
        }

        private void ServiceEditView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ServiceEditViewModel oldVm)
            {
                oldVm.RequestClose -= Close;
            }

            if (e.NewValue is ServiceEditViewModel newVm)
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