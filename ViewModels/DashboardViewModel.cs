using BlockHouse.Api;
using BlockHouse.Helpers; // Thêm using cho AppLogger
using BlockHouse.Models.Responses;
using BlockHouse.ViewModels.Components;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using static BlockHouse.ViewModels.MainWindowViewModel;

namespace BlockHouse.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private int _totalServiceSales;
        private decimal _totalRevenue;
        private decimal _growthPercent;
        private List<EmployeeRanking> _topEmployees;
        private SeriesCollection _revenueSeries;
        private string[] _months;
        private bool _isLoading;

        public int TotalServiceSales
        {
            get => _totalServiceSales;
            set => SetProperty(ref _totalServiceSales, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        public decimal GrowthPercent
        {
            get => _growthPercent;
            set => SetProperty(ref _growthPercent, value);
        }

        public List<EmployeeRanking> TopEmployees
        {
            get => _topEmployees;
            set => SetProperty(ref _topEmployees, value);
        }

        public SeriesCollection RevenueSeries
        {
            get => _revenueSeries;
            set => SetProperty(ref _revenueSeries, value);
        }

        public string[] Months
        {
            get => _months;
            set => SetProperty(ref _months, value);
        }

        public Func<double, string> YFormatter { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly DashboardApi _dashboardApi;

        public DashboardViewModel()
        {
            try
            {
                AppLogger.Instance.LogInfo("Khởi tạo DashboardViewModel");
                _dashboardApi = new DashboardApi();
                YFormatter = value => (value / 1000000).ToString("N1") + "M";

                TopEmployees = new List<EmployeeRanking>();
                RevenueSeries = new SeriesCollection();
                Months = Array.Empty<string>();

                // Load data on initialization
                _ = LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Exception in DashboardViewModel constructor", ex);
                // Set default values to prevent null references
                TopEmployees = new List<EmployeeRanking>();
                RevenueSeries = new SeriesCollection();
                Months = new[] { "Tháng 1", "Tháng 2", "Tháng 3" };
            }
        }

        public async Task LoadDashboardDataAsync(int? month = 8, int? year = 2025)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu tải dữ liệu dashboard: month={month}, year={year}");
            try
            {
                IsLoading = true;

                var response = await _dashboardApi.GetDashboardData();

                if (response.IsSuccess && response.Data != null)
                {
                    AppLogger.Instance.LogInfo("Tải dữ liệu dashboard thành công");
                    ProcessApiResponse(response.Data);
                }
                else
                {
                    AppLogger.Instance.LogError($"Lỗi tải dữ liệu dashboard: {response.Message}");
                    CustomMessageBox.Show(
                        "Lấy dữ liệu không thành công!",
                        new List<string> { "OK" },
                        MessageType.Error
                    );
                    WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError("Exception khi tải dữ liệu dashboard", ex);
                CustomMessageBox.Show(
                    "Lấy dữ liệu không thành công!",
                    new List<string> { "OK" },
                    MessageType.Error
                );
                WeakReferenceMessenger.Default.Send(new ToggleOverlayMessage { IsVisible = false });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ProcessApiResponse(DashboardResponse data)
        {
            AppLogger.Instance.LogInfo("Xử lý dữ liệu dashboard từ API");
            // Map data from API
            TotalServiceSales = data.TotalServiceSales;
            TotalRevenue = (decimal)data.TotalRevenue;
            GrowthPercent = (decimal)data.GrowthPercent;

            // Process top employees
            TopEmployees = data.TopEmployees
                .Select((e, index) => new EmployeeRanking
                {
                    Rank = index + 1,
                    Name = e.Name,
                    OrderCount = e.OrderCount,
                    Revenue = (decimal)e.Revenue
                })
                .ToList();

            // Process revenue chart for each service
            RevenueSeries = new SeriesCollection();
            var allMonths = Enumerable.Range(1, 12).Select(m => $"Tháng {m}").ToArray();
            Months = allMonths;

            foreach (var service in data.MonthlyChart)
            {
                var monthlyData = service.MonthlyData
                    .OrderBy(m => m.Month)
                    .ToList();

                var revenues = new ChartValues<decimal>();
                for (int i = 1; i <= 12; i++)
                {
                    var monthData = monthlyData.FirstOrDefault(m => m.Month == i);
                    revenues.Add(monthData != null ? (decimal)monthData.Revenue : 0m);
                }

                RevenueSeries.Add(new LineSeries
                {
                    Title = service.ServiceName,
                    Values = revenues,
                    PointGeometry = null,
                    StrokeThickness = 3,
                    Fill = Brushes.Transparent,
                    Stroke = GetColorForService(service.ServiceId),
                    LineSmoothness = 1
                });
            }
        }

        private SolidColorBrush GetColorForService(int serviceId)
        {
            // Assign distinct colors for each service
            var colors = new[]
            {
                Color.FromRgb(0, 150, 136), // Teal
                Color.FromRgb(255, 87, 34), // Orange
                Color.FromRgb(33, 150, 243), // Blue
                Color.FromRgb(156, 39, 176), // Purple
                Color.FromRgb(255, 193, 7), // Amber
                Color.FromRgb(76, 175, 80), // Green
                Color.FromRgb(233, 30, 99) // Pink
            };
            return new SolidColorBrush(colors[serviceId % colors.Length]);
        }
    }

    public class EmployeeRanking
    {
        public int Rank { get; set; }
        public string RankDisplay => $"{Rank}. {Name}";
        public string Name { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }

        public string RankImagePath => $"/Assets/Top{Rank}.png";
    }
}