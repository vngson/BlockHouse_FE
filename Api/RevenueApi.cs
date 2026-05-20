using BlockHouse.Helpers;
using BlockHouse.Models.Responses;
using System.Net.Http;

namespace BlockHouse.Api
{
    class RevenueApi
    {
        private static readonly string prefixPath = "api/v1/orders";
        private static readonly string baseUrl = "http://127.0.0.1:5000/";

        public RevenueApi()
        {
            ApiHelper.Initialize(baseUrl); // Gọi Initialize thay vì ConfigureHttpClient
        }

        public Task<GetResponseApi<OrderListResponse>> GetOrdersByEmployee(
    int? page = 1,
    int? pageSize = 10,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string? Keyword = null,
    bool isShowLoading = true
)
        {
            var url = $"{prefixPath}";

            // Chuẩn bị body JSON
            var body = new Dictionary<string, object>();

            if (page.HasValue)
                body["page"] = page.Value;

            if (pageSize.HasValue)
                body["page_size"] = pageSize.Value;

            if (startDate.HasValue)
                body["date_from"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                body["date_to"] = endDate.Value.ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(Keyword))
                body["keyword"] = Keyword;

            return ApiHelper.SendApi<OrderListResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }


        public Task<GetResponseApi<OrdersByDateResponse>> GetOrdersByDate(
        int? page = 1,
        int? pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? Keyword = null,
        bool isShowLoading = true
    )
        {
            var url = $"{prefixPath}/by_date";

            // Chuẩn bị body JSON
            var body = new Dictionary<string, object>();

            if (page.HasValue)
                body["page"] = page.Value;

            if (pageSize.HasValue)
                body["page_size"] = pageSize.Value;

            if (startDate.HasValue)
                body["date_from"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                body["date_to"] = endDate.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(Keyword))
                body["keyword"] = Keyword;

            return ApiHelper.SendApi<OrdersByDateResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public Task<GetResponseApi<OrdersByEmployeeAndDateResponse>> GetOrdersByEmployeeAndDate(
        int? page = 1,
        int? pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? Keyword = null,
        bool isShowLoading = true
    )
        {
            var url = $"{prefixPath}/revenue_by_employee";

            // Chuẩn bị body JSON
            var body = new Dictionary<string, object>();

            if (page.HasValue)
                body["page"] = page.Value;

            if (pageSize.HasValue)
                body["page_size"] = pageSize.Value;

            if (startDate.HasValue)
                body["date_from"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                body["date_to"] = endDate.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(Keyword))
                body["keyword"] = Keyword;

            return ApiHelper.SendApi<OrdersByEmployeeAndDateResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public Task<GetResponseApi<OrdersByEmployeeAndMonthResponse>> GetOrdersByEmployeeAndMonth(
        int? page = 1,
        int? pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? Keyword = null,
        bool isShowLoading = true
    )
        {
            var url = $"{prefixPath}/revenue_monthly_by_employee";

            // Chuẩn bị body JSON
            var body = new Dictionary<string, object>();

            if (page.HasValue)
                body["page"] = page.Value;

            if (pageSize.HasValue)
                body["page_size"] = pageSize.Value;

            if (startDate.HasValue)
                body["date_from"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                body["date_to"] = endDate.Value.ToString("yyyy-MM-dd");

            if (!string.IsNullOrEmpty(Keyword))
                body["keyword"] = Keyword;

            return ApiHelper.SendApi<OrdersByEmployeeAndMonthResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public async Task<GetResponseApi<OrderResponse>> GetOrderById(int id, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{id}";
            return await ApiHelper.SendApi<OrderResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                null
            );
        }

        public async Task<GetResponseApi<OrderOneDateData>> GetOrderForOneDate(string date, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/for_one_date";
            var body = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(date))
                body["order_date"] = date;
            return await ApiHelper.SendApi<OrderOneDateData>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public async Task<GetResponseApi<OrderByDateAndEmployeeResponse>> GetOrdersByDateAndEmployee(
            string orderDate,
            int employeeId,
            bool isShowLoading = true
        )
        {
            var url = $"{prefixPath}/by_date_and_employee";
            var body = new Dictionary<string, object>
            {
                { "order_date", orderDate },
                { "employee_id", employeeId }
            };

            return await ApiHelper.SendApi<OrderByDateAndEmployeeResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public async Task<GetResponseApi<string>> DeleteOrderById(int id, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{id}";
            return await ApiHelper.SendApi<string>(
                HttpMethod.Delete,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                null
            );
        }

        public async Task<GetResponseApi<OrderResponse>> CreateOrder(
            object data,
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/create";
            return await ApiHelper.SendApi<OrderResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                data,
                null
            );
        }

        public async Task<GetResponseApi<OrderResponse>> UpdateOrder(int orderId, object data,
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{orderId}";
            return await ApiHelper.SendApi<OrderResponse>(
                HttpMethod.Put,
                url,
                isShowLoading,
                CancellationToken.None,
                data,
                null
            );
        }

    }

}
