using BlockHouse.Helpers;
using BlockHouse.Models;
using BlockHouse.ViewModels.Popup.Service;
using System.Net.Http;

namespace BlockHouse.Api
{
    public class ServiceApi
    {
        private static readonly string prefixPath = "/api/v1/service";
        private static readonly string baseUrl = "http://127.0.0.1:5000/";

        public ServiceApi()
        {
            ApiHelper.Initialize(baseUrl);
        }

        public Task<GetResponseApi<ServiceListResponse>> GetAllServices(
            int? page = 1,
            int? pageSize = 15,
            string? keyword = null,  // Thêm tham số keyword
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/";

            // Prepare query parameters
            var queryParams = new ServiceFilterRequest
            {
                page = page ?? 1,
                page_size = pageSize ?? 15,
                keyword = keyword  // Thêm keyword
            };

            return ApiHelper.SendApi<ServiceListResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                queryParams,
                null
            );
        }

        public Task<GetResponseApi<ServiceListResponse>> GetAllServicesNoPagination(
           bool isShowLoading = true)
        {
            var url = $"{prefixPath}/all";

            return ApiHelper.SendApi<ServiceListResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                null
            );
        }

        public Task<GetResponseApi<ServiceResponse>> GetServiceById(
            int serviceId,
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{serviceId}";

            return ApiHelper.SendApi<ServiceResponse>(
                HttpMethod.Get,
                url,
                isShowLoading,
                CancellationToken.None,
                null,
                null
            );
        }

        public Task<GetResponseApi<ServiceResponse>> CreateService(
            ServiceAddRequest serviceData,
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/";

            return ApiHelper.SendApi<ServiceResponse>(
                HttpMethod.Post,
                url,
                isShowLoading,
                CancellationToken.None,
                serviceData,
                null
            );
        }

        public Task<GetResponseApi<ServiceResponse>> UpdateService(
            int serviceId,
            ViewModels.Popup.Service.ServiceUpdateRequest serviceData,
            bool isShowLoading = true)
        {
            var url = $"{prefixPath}/{serviceId}";

            return ApiHelper.SendApi<ServiceResponse>(
                HttpMethod.Put,
                url,
                isShowLoading,
                CancellationToken.None,
                serviceData,
                null
            );
        }
    }
}
