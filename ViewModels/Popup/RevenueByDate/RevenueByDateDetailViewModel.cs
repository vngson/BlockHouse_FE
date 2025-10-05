using BlockHouse.Api;
using BlockHouse.Helpers; // Thêm using cho AppLogger
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;
using static BlockHouse.ViewModels.RevenueViewModel;

namespace BlockHouse.ViewModels.Popup
{
    class RevenueByDateDetailViewModel : ObservableObject, IDisposable
    {
        private readonly RevenueApi _revenueApi;
        private bool _disposed;


        private string _datetime;
        public string Datetime
        {
            get => _datetime;
            set
            {
                if (_datetime != value)
                {
                    _datetime = value;
                    OnPropertyChanged(nameof(Datetime));
                }
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private int _orderId;
        public int OrderId
        {
            get => _orderId;
            set
            {
                if (_orderId != value)
                {
                    _orderId = value;
                    OnPropertyChanged(nameof(OrderId));
                }
            }
        }

        private int? _promotionId;
        public int? PromotionId
        {
            get => _promotionId;
            set
            {
                if (_promotionId != value)
                {
                    _promotionId = value;
                    OnPropertyChanged(nameof(PromotionId));
                }
            }
        }

        private List<ServiceSummary> _services;
        public List<ServiceSummary> Services
        {
            get => _services;
            set
            {
                if (_services != value)
                {
                    _services = value;
                    OnPropertyChanged(nameof(Services));
                    OnPropertyChanged(nameof(ServicesDisplay));
                }
            }
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(TotalDisplay));
                }
            }
        }

        public string TotalDisplay => string.Format("{0:N0} VNĐ", Total);

        public string ServicesDisplay
        {
            get
            {
                if (Services == null || Services.Count == 0)
                    return string.Empty;

                return string.Join(", ", Services.Select(s => $"{s.Name} ({s.Quantity})"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public ICommand CloseCommand { get; }

        public event Action RequestClose;

        public RevenueByDateDetailViewModel()
        {
            AppLogger.Instance.LogInfo("Khởi tạo RevenueByDateDetailViewModel");
            _revenueApi = new RevenueApi();
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        public async Task LoadDataAsync(string dateTime)
        {
            AppLogger.Instance.LogInfo($"Bắt đầu LoadDataAsync cho ngày {dateTime}");
            try
            {
                var response = await _revenueApi.GetOrderForOneDate(dateTime);
                if (response.IsSuccess && response.Data != null && response.Data.OrdersByDate.Any())
                {
                    AppLogger.Instance.LogInfo($"Tải chi tiết doanh thu theo ngày thành công: {dateTime}");
                    var order = response.Data.OrdersByDate.First();

                    Datetime = order.Date.ToString("dd-MM-yyyy");
                    Services = order.Services.Select(s => new ServiceSummary
                    {
                        Name = s.Name,
                        Quantity = s.Quantity
                    }).ToList();

                    Total = (decimal)order.Total;
                }
                else
                {
                    AppLogger.Instance.LogWarning($"Không tìm thấy dữ liệu doanh thu cho ngày {dateTime}: {response.Message}");
                    Datetime = "N/A";
                    Services = new List<ServiceSummary>();
                    Total = 0;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.LogError($"Lỗi khi tải chi tiết doanh thu theo ngày {dateTime}", ex);
                Datetime = "N/A";
                Services = new List<ServiceSummary>();
                Total = 0;
            }
        }


        private void ExecuteClose()
        {
            AppLogger.Instance.LogInfo("Đóng RevenueByDateDetailViewModel");
            RequestClose?.Invoke();
            Dispose(); // Gọi Dispose khi đóng ViewModel
        }

        // Triển khai IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Ngăn GC gọi finalizer nếu đã dispose
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                AppLogger.Instance.LogInfo("Dispose RevenueByDateDetailViewModel");
                // Giải phóng tài nguyên được quản lý
                if (_revenueApi is IDisposable disposableApi)
                {
                    disposableApi.Dispose();
                }

                // Giải phóng các tài nguyên khác nếu có
                // Ví dụ: Hủy đăng ký sự kiện, giải phóng danh sách, v.v.
                Services?.Clear();
                RequestClose = null; // Hủy đăng ký sự kiện
            }

            // Giải phóng tài nguyên không được quản lý (nếu có)
            // Ví dụ: Đóng kết nối mạng, file handler, v.v.

            _disposed = true;
        }

        // Finalizer (chỉ cần nếu có tài nguyên không được quản lý)
        ~RevenueByDateDetailViewModel()
        {
            Dispose(false);
        }
    }
}