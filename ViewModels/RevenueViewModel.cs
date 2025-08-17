using BlockHouse.Api;
using BlockHouse.Models;
using BlockHouse.Models.Responses;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Windows.Media;

namespace BlockHouse.ViewModels
{
    public class RevenueViewModel : ViewModelBase
    {

        public class ServiceSummary
        {
            public string Name { get; set; }   
            public int Quantity { get; set; }  
        }

        public class OrderItem
        {
            public DateTime Datetime { get; set; }
            public string EmployeeName { get; set; }
            public int Id { get; set; }
            public int? PromotionId { get; set; }
            public List<ServiceSummary> Services { get; set; }
            public decimal Total { get; set; }

            public string ServicesDisplay
            {
                get
                {
                    if (Services == null || Services.Count == 0)
                        return string.Empty;

                    return string.Join(", ", Services.Select(s => $"{s.Name} x{s.Quantity}"));
                }
            }
        }
        private List<OrderItem> _revenueItems;

        public List<OrderItem> RevenueItems
        {
            get => _revenueItems;
            set => SetProperty(ref _revenueItems, value);
        }

        public ICommand ViewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private readonly RevenueApi _revenueApi;

        public RevenueViewModel()
        {
            _revenueApi = new RevenueApi();
            // Tạo dữ liệu giả
            RevenueItems = new List<OrderItem>();

            // Khởi tạo các lệnh
            ViewCommand = new RelayCommand<OrderResponse>(ExecuteView);
            EditCommand = new RelayCommand<OrderResponse>(ExecuteEdit);
            DeleteCommand = new RelayCommand<OrderResponse>(ExecuteDelete);

            _ = LoadData();
        }

        private void ExecuteView(OrderResponse item)
        {
            // Hàm xử lý lệnh Xem
        }

        private void ExecuteEdit(OrderResponse item)
        {
            // Hàm xử lý lệnh Sửa
        }

        private void ExecuteDelete(OrderResponse item)
        {
            // Hàm xử lý lệnh Xóa
        }

        private async Task LoadData()
        {
            try
            {
                IsLoading = true;

                var response = await _revenueApi.GetOrdersByEmployee();

                if (response.IsSuccess && response.Data != null)
                {
                    RevenueItems = response.Data.Orders.Select((o, index) => new OrderItem
                    {
                        Id = index + 1,
                        Datetime = o.DateTime,
                        EmployeeName = o.EmployeeName ?? "",
                        PromotionId = o.PromotionId,
                        Services = o.Services.Select(s => new ServiceSummary
                        {
                            Name = s.Name,
                            Quantity = s.Quantity
                        }).ToList(),
                        Total = (decimal)(o.Total ?? 0)
                    }).ToList();
                }
                else
                {
                    // Handle error (could display a message)
                    Console.WriteLine($"Error loading dashboard data: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when loading dashboard data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }

        }
    }
}