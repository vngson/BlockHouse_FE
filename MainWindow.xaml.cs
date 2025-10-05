using BlockHouse.Services;
using BlockHouse.ViewModels;
using BlockHouse.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;

namespace BlockHouse
{
    public partial class MainWindow : Window
    {
        private BackendProcessService _backendService = new BackendProcessService();
        private SplashWindow _splashWindow;
        private CancellationTokenSource _startupCts;

        public MainWindow()
        {
            // Ẩn MainWindow ban đầu
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;

            InitializeComponent();

            // Khởi động backend và hiển thị splash
            InitializeBackendAndUI();
        }

        private async void InitializeBackendAndUI()
        {
            _startupCts = new CancellationTokenSource();

            // Hiển thị overlay (giống PrepressFE)
            WeakReferenceMessenger.Default.Send(new MainWindowViewModel.ToggleOverlayMessage { IsVisible = true });

            // Hiển thị splash window
            _splashWindow = new SplashWindow();
            _splashWindow.Show();

            // Khởi động backend và kiểm tra health
            bool backendStarted = await _backendService.StartBackendWithHealthCheck(_startupCts.Token);

            if (backendStarted)
            {
                // Backend ready, khởi tạo MainWindow bình thường
                _splashWindow.Close();
                this.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Maximized;
                this.ShowInTaskbar = true;

                // Ẩn overlay
                WeakReferenceMessenger.Default.Send(new MainWindowViewModel.ToggleOverlayMessage { IsVisible = false });

                // Khởi tạo ViewModel
                DataContext = new MainWindowViewModel();
            }
            else
            {
                // Backend không sẵn sàng, thoát ứng dụng
                _splashWindow.Close();
                MessageBox.Show("Cannot start backend. Application will exit.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            _startupCts?.Dispose();
            _startupCts = null;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Hủy startup process nếu đang chạy
            _startupCts?.Cancel();
            _startupCts?.Dispose();

            // Kill backend process khi đóng main window (giống PrepressFE)
            _backendService.KillBackendProcess();
            base.OnClosed(e);
        }

        // Thêm phương thức ShowLoading để tương thích với pattern PrepressFE
        public void ShowLoading(bool show)
        {
            // Bạn có thể tích hợp với overlay hiện có
            WeakReferenceMessenger.Default.Send(new MainWindowViewModel.ToggleOverlayMessage { IsVisible = show });
        }
    }
}