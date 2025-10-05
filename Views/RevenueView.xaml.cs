using BlockHouse.ViewModels;
using System.Windows.Controls;

namespace BlockHouse.Views
{
    /// <summary>
    /// Interaction logic for RevenueView.xaml
    /// </summary>
    public partial class RevenueView : UserControl
    {
        public RevenueView()
        {
            InitializeComponent();
            DataContext = new RevenueViewModel();
        }
    }
}
