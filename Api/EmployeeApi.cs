using BlockHouse.Helpers;
using BlockHouse.Models.Responses;
using BlockHouse.ViewModels.Popup.Employee;
using System.Net.Http;

namespace BlockHouse.Api
{
    class EmployeeApi
    {
        private static readonly string prefixPath = "/api/v1/employees";
        private static readonly string baseUrl = "http://127.0.0.1:5000/";


        public EmployeeApi()
        {
            ApiHelper.Initialize(baseUrl); // Gọi Initialize thay vì ConfigureHttpClient
        }

        public Task<GetResponseApi<EmployeeListResponse>> GetEmployees(
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


            return ApiHelper.SendApi<EmployeeListResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                body,
                null
            );
        }

        public async Task<GetResponseApi<EmployeeResponse>> GetEmployeeById(int id, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{id}";
            return await ApiHelper.SendApi<EmployeeResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                null
            );
        }

        public async Task<GetResponseApi<EmployeeResponse>> UpdateEmployee(int id, int status, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{id}";
            var request = new { status = status };
            return await ApiHelper.SendApi<EmployeeResponse>(
                HttpMethod.Put,
                url,
                isShowLoading,
                CancellationToken.None,
                request,
                null
            );
        }

        public async Task<GetResponseApi<EmployeeResponse>> AddEmployee(EmployeeAddRequest newEmployee, bool isShowLoading = true)
        {
            var url = $"{prefixPath}/create_employee";
            return await ApiHelper.SendApi<EmployeeResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                newEmployee,
                null
            );
        }
    }
}
