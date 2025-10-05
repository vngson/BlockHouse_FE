using BlockHouse.ViewModels;
using System.Windows.Controls;

namespace BlockHouse.Views
{
    /// <summary>
    /// Interaction logic for ServicesView.xaml
    /// </summary>
    public partial class ServicesView : UserControl
    {
        public ServicesView()
        {
            InitializeComponent();
            DataContext = new ServicesViewModel();
        }
    }
}
