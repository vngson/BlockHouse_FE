using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BlockHouse.Helpers;
using BlockHouse.Models.Responses;

namespace BlockHouse.Api
{
    public class DashboardApi
    {
        private static readonly string prefixPath = "api/v1/dashboard";
        private static readonly string baseUrl = "http://127.0.0.1:5000/";

        public DashboardApi()
        {
            ApiHelper.Initialize(baseUrl); // Gọi Initialize thay vì ConfigureHttpClient
        }

        public Task<GetResponseApi<DashboardResponse>> GetDashboardData(int? month = null, int? year = null, bool isShowLoading = true)
        {
            var url = $"{prefixPath}";
            var queryParams = new Dictionary<string, string>();

            if (month.HasValue)
            {
                if (month < 1 || month > 12)
                {
                    return Task.FromResult(new GetResponseApi<DashboardResponse>
                    {
                        Message = "Month must be between 1 and 12",
                        Data = null,
                        StatusCode = 400
                    });
                }
                queryParams.Add("month", month.Value.ToString());
            }

            if (year.HasValue)
            {
                if (year < 2000 || year > DateTime.Now.Year + 1)
                {
                    return Task.FromResult(new GetResponseApi<DashboardResponse>
                    {
                        Message = "Year must be between 2000 and next year",
                        Data = null,
                        StatusCode = 400
                    });
                }
                queryParams.Add("year", year.Value.ToString());
            }

            return ApiHelper.SendApi<DashboardResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                queryParams.Count > 0 ? queryParams : null
            );
        }

        public Task<GetResponseApi<DashboardResponse>> GetCurrentDashboardData(bool isShowLoading = true)
        {
            return GetDashboardData(null, null, isShowLoading);
        }
    }
}