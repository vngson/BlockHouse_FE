using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace BlockHouse.Models.Responses
{
    public class DashboardResponse : INotifyPropertyChanged
    {
        private List<EmployeeRevenue> _topEmployees;
        private double _totalRevenue;
        private int _totalServiceSales;
        private double _growthPercent;
        private List<ServiceRevenue> _monthlyChart;

        [JsonProperty("top_employees")]
        public List<EmployeeRevenue> TopEmployees
        {
            get => _topEmployees;
            set { _topEmployees = value; OnPropertyChanged(nameof(TopEmployees)); }
        }

        [JsonProperty("total_revenue")]
        public double TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(nameof(TotalRevenue)); }
        }

        [JsonProperty("total_service_sales")]
        public int TotalServiceSales
        {
            get => _totalServiceSales;
            set { _totalServiceSales = value; OnPropertyChanged(nameof(TotalServiceSales)); }
        }

        [JsonProperty("growth_percent")]
        public double GrowthPercent
        {
            get => _growthPercent;
            set { _growthPercent = value; OnPropertyChanged(nameof(GrowthPercent)); }
        }

        [JsonProperty("monthly_chart")]
        public List<ServiceRevenue> MonthlyChart
        {
            get => _monthlyChart;
            set { _monthlyChart = value; OnPropertyChanged(nameof(MonthlyChart)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class EmployeeRevenue : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private double _revenue;
            private int _orderCount;

            [JsonProperty("id")]
            public int Id
            {
                get => _id;
                set { _id = value; OnPropertyChanged(nameof(Id)); }
            }

            [JsonProperty("name")]
            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(nameof(Name)); }
            }

            [JsonProperty("revenue")]
            public double Revenue
            {
                get => _revenue;
                set { _revenue = value; OnPropertyChanged(nameof(Revenue)); }
            }

            [JsonProperty("order_count")]
            public int OrderCount
            {
                get => _orderCount;
                set { _orderCount = value; OnPropertyChanged(nameof(OrderCount)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class ServiceRevenue : INotifyPropertyChanged
        {
            private int _serviceId;
            private string _serviceName;
            private List<MonthlyRevenue> _monthlyData;

            [JsonProperty("service_id")]
            public int ServiceId
            {
                get => _serviceId;
                set { _serviceId = value; OnPropertyChanged(nameof(ServiceId)); }
            }

            [JsonProperty("service_name")]
            public string ServiceName
            {
                get => _serviceName;
                set { _serviceName = value; OnPropertyChanged(nameof(ServiceName)); }
            }

            [JsonProperty("monthly_data")]
            public List<MonthlyRevenue> MonthlyData
            {
                get => _monthlyData;
                set { _monthlyData = value; OnPropertyChanged(nameof(MonthlyData)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class MonthlyRevenue : INotifyPropertyChanged
        {
            private int _month;
            private double _revenue;
            private int _quantity;

            [JsonProperty("month")]
            public int Month
            {
                get => _month;
                set { _month = value; OnPropertyChanged(nameof(Month)); }
            }

            [JsonProperty("revenue")]
            public double Revenue
            {
                get => _revenue;
                set { _revenue = value; OnPropertyChanged(nameof(Revenue)); }
            }

            [JsonProperty("quantity")]
            public int Quantity
            {
                get => _quantity;
                set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}