using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace BlockHouse.Helpers
{
    public static class ApiHelper
    {
        private static readonly HttpClient _httpClient;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            // Equivalent to System.Text.Json's PropertyNameCaseInsensitive
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
            {
                NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
            }
        };
        private static bool _isInitialized = false;

        static ApiHelper()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public static void Initialize(string baseAddress, string authToken = null)
        {
            if (_isInitialized)
            {
                return;
            }

            _httpClient.BaseAddress = new Uri(baseAddress);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);
            }

            _isInitialized = true;
        }

        public static async Task<GetResponseApi<T>> SendApi<T>(
            HttpMethod method,
            string url,
            bool isShowLoading,
            CancellationToken cancellationToken,
            object body = null,
            Dictionary<string, string> queryParams = null)
        {
            try
            {
                if (isShowLoading)
                {
                    // Code hiển thị loading UI ở đây
                }

                if (queryParams != null && queryParams.Count > 0)
                {
                    var query = HttpUtility.ParseQueryString(string.Empty);
                    foreach (var param in queryParams)
                    {
                        query[param.Key] = param.Value;
                    }
                    url += $"?{query}";
                }

                var request = new HttpRequestMessage(method, url);

                if (body != null)
                {
                    var jsonContent = JsonConvert.SerializeObject(body, _jsonSettings);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return GetResponseApi<T>.Error(
                        $"API request failed: {content}",
                        response.StatusCode);
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content, _jsonSettings);
                if (apiResponse == null || apiResponse.Data == null)
                {
                    return GetResponseApi<T>.Error(
                        "API response is empty or invalid",
                        HttpStatusCode.BadRequest);
                }

                return GetResponseApi<T>.Success(apiResponse.Data, apiResponse.Message);
            }
            catch (JsonException jsonEx)
            {
                return GetResponseApi<T>.Error(
                    $"Failed to deserialize API response: {jsonEx.Message}",
                    HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return GetResponseApi<T>.Error(
                    $"An unexpected error occurred: {ex.Message}",
                    HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (isShowLoading)
                {
                    // Code ẩn loading UI ở đây
                }
            }
        }
    }
}