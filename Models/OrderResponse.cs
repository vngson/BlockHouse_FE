using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

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
}