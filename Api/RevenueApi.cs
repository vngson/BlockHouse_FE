using BlockHouse.Helpers;
using BlockHouse.Models.Responses;
using System.Net.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public class OrderListResponse
        {
            public List<OrderResponse> Orders { get; set; }
            public int Page { get; set; }
            public int Page_Size { get; set; }
            public int Total_Orders { get; set; }
        }

        public Task<GetResponseApi<OrderListResponse>> GetOrdersByEmployee(
    int? page = 1,
    int? pageSize = 10,
    DateTime? startDate = null,
    DateTime? endDate = null,
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
                body["start_date"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                body["end_date"] = endDate.Value.ToString("yyyy-MM-dd");

            return ApiHelper.SendApi<OrderListResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,   
                null
            );
        }
    }
}
