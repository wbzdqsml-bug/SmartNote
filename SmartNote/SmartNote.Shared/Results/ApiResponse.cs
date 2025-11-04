namespace SmartNote.Shared.Results
{
    /// <summary>
    /// 通用 API 响应包装类
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 状态码（业务意义层面的码，不是HTTP码）
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 响应数据对象
        /// </summary>
        public object? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(int code, string message, object? data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// 成功响应（无数据）
        /// </summary>
        public static ApiResponse Success(string message = "操作成功") =>
            new ApiResponse(0, message);

        /// <summary>
        /// 成功响应（带数据）
        /// </summary>
        public static ApiResponse Success(object? data, string message = "操作成功") =>
            new ApiResponse(0, message, data);

        /// <summary>
        /// 失败响应（默认业务错误）
        /// </summary>
        public static ApiResponse Fail(string message, int code = -1) =>
            new ApiResponse(code, message);

        /// <summary>
        /// 创建通用响应
        /// </summary>
        public static ApiResponse Create(int code, string message, object? data = null) =>
            new ApiResponse(code, message, data);
    }

    /// <summary>
    /// 泛型版 API 响应，方便强类型返回
    /// </summary>
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(int code, string message, T? data = default)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Success(T data, string message = "操作成功") =>
            new ApiResponse<T>(0, message, data);

        public static ApiResponse<T> Fail(string message, int code = -1) =>
            new ApiResponse<T>(code, message);
    }
}
