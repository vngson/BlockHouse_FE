using System.Net;

namespace BlockHouse.Helpers
{
    public class GetResponseApi<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

        public GetResponseApi()
        {
        }

        public GetResponseApi(T data, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Data = data;
            Message = message;
            StatusCode = (int)statusCode;
        }

        public static GetResponseApi<T> Success(T data, string message = "")
        {
            return new GetResponseApi<T>
            {
                Data = data,
                Message = message,
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public static GetResponseApi<T> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new GetResponseApi<T>
            {
                Data = default,
                Message = message,
                StatusCode = (int)statusCode
            };
        }
    }
}