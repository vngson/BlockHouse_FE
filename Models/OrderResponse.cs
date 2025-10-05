using Newtonsoft.Json;
using System.ComponentModel;
using static BlockHouse.Models.Responses.OrderResponse;

namespace BlockHouse.Models.Responses
{
    public class OrderResponse : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _datetime;
        private int? _employeeId;
        private string _employeeName;
        private int? _promotionId;
        private double? _total;
        private List<OrderServiceResponse> _services;

        [JsonProperty("id")]
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        [JsonProperty("datetime")]
        public DateTime DateTime
        {
            get => _datetime;
            set { _datetime = value; OnPropertyChanged(nameof(DateTime)); }
        }

        [JsonProperty("employee_id")]
        public int? EmployeeId
        {
            get => _employeeId;
            set { _employeeId = value; OnPropertyChanged(nameof(EmployeeId)); }
        }

        [JsonProperty("employee_name")]
        public string EmployeeName
        {
            get => _employeeName;
            set { _employeeName = value; OnPropertyChanged(nameof(EmployeeName)); }
        }

        [JsonProperty("promotion_id")]
        public int? PromotionId
        {
            get => _promotionId;
            set { _promotionId = value; OnPropertyChanged(nameof(PromotionId)); }
        }

        [JsonProperty("total")]
        public double? Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(nameof(Total)); }
        }

        [JsonProperty("services")]
        public List<OrderServiceResponse> Services
        {
            get => _services;
            set { _services = value; OnPropertyChanged(nameof(Services)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class OrderServiceResponse : INotifyPropertyChanged
        {
            private int _serviceId;
            private int _quantity;
            private string _name;

            [JsonProperty("service_id")]
            public int ServiceId
            {
                get => _serviceId;
                set { _serviceId = value; OnPropertyChanged(nameof(ServiceId)); }
            }

            [JsonProperty("quantity")]
            public int Quantity
            {
                get => _quantity;
                set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
            }

            [JsonProperty("name")]
            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(nameof(Name)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class OrderByDate
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("services")]
        public List<OrderServiceResponse> Services { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }
    }

    public class OrderOneDateData
    {
        [JsonProperty("order_date")]
        public string OrderDate { get; set; }

        [JsonProperty("orders_by_date")]
        public List<OrderByDate> OrdersByDate { get; set; }

        [JsonProperty("total_orders")]
        public int TotalOrders { get; set; }
    }

    public class OrderByEmployeeAndDate
    {

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("employee_id")]
        public int EmployeeId { get; set; }

        [JsonProperty("employee_name")]
        public string EmployeeName { get; set; }

        [JsonProperty("services")]
        public List<OrderServiceResponse> Services { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }
    }

    public class OrderByDateAndEmployeeResponse
    {
        [JsonProperty("order_data")]
        public OrderByEmployeeAndDate OrderData { get; set; }
        [JsonProperty("total_orders")]
        public int TotalOrders { get; set; }
    }

    public class OrderListResponse
    {
        public List<OrderResponse> Orders { get; set; }
        public int Page { get; set; }
        public int Page_Size { get; set; }
        public int Total_Orders { get; set; }
    }

    public class OrderByDateResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("employee_id")]
        public int? EmployeeId { get; set; }

        [JsonProperty("promotion_id")]
        public int? PromotionId { get; set; }

        [JsonProperty("services")]
        public List<OrderServiceResponse> Services { get; set; } = new List<OrderServiceResponse>();
    }

    public class OrdersByDateResponse
    {
        [JsonProperty("orders_by_date")]
        public List<OrderByDateResponse> OrdersByDate { get; set; } = new List<OrderByDateResponse>();

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("page_size")]
        public int PageSize { get; set; }

        [JsonProperty("total_days")]
        public int TotalDays { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }


    public class OrderByEmployeeAndDateResponse
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; } // Or use DateOnly if .NET 6+
        [JsonProperty("employee_id")]
        public int EmployeeId { get; set; } // Non-nullable, as filtered in service
        [JsonProperty("employee_name")]
        public string EmployeeName { get; set; }
        [JsonProperty("total")]
        public decimal Total { get; set; }
        [JsonProperty("services")]
        public List<OrderServiceResponse> Services { get; set; } = new List<OrderServiceResponse>();
    }

    public class OrdersByEmployeeAndDateResponse
    {
        [JsonProperty("revenue_by_employee")]
        public List<OrderByEmployeeAndDateResponse> OrdersByEmployeeAndDate { get; set; } = new List<OrderByEmployeeAndDateResponse>();
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("page_size")]
        public int PageSize { get; set; }
        [JsonProperty("total_date_employees")]
        public int TotalDays { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}