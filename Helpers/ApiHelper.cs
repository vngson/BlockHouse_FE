using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

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
            // Accept both application/json and application/json; charset=utf-8 to avoid Content-Type warnings
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json", 0.9) { CharSet = "utf-8" });

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

                // Nếu API thành công nhưng không có body (DELETE, 204, v.v.)
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return GetResponseApi<T>.Success(default, "Request successful (no content)");
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    // Nếu 200 mà body rỗng → vẫn coi là success
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        return GetResponseApi<T>.Success(default, "Request successful (empty body)");
                    }

                    // Nếu có body mà không phải JSON → trả về raw string
                    var contentType = response.Content.Headers.ContentType?.ToString();
                    if (string.IsNullOrEmpty(contentType) || !contentType.Contains("application/json"))
                    {
                        return GetResponseApi<T>.Success((T)(object)content, "Request successful (non-JSON response)");
                    }

                    // Deserialize theo chuẩn
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content, _jsonSettings);

                    if (apiResponse == null)
                    {
                        return GetResponseApi<T>.Error("API response is empty or invalid", HttpStatusCode.BadRequest);
                    }

                    return GetResponseApi<T>.Success(apiResponse.Data, apiResponse.Message ?? "Request successful");
                }

                // Trường hợp không thành công
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    // Parse error JSON theo cùng cấu trúc ApiResponse
                    var errorObj = JsonConvert.DeserializeObject<ApiResponse<object>>(errorContent, _jsonSettings);

                    var errMsg = errorObj?.Message ?? "Unknown error";

                    return GetResponseApi<T>.Error(
                        errMsg,
                        response.StatusCode
                    );
                }
                catch
                {
                    // Nếu parse fail (không phải JSON) thì fallback
                    return GetResponseApi<T>.Error(
                        errorContent,
                        response.StatusCode
                    );
                }

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