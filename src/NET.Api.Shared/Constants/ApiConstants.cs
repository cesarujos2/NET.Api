namespace NET.Api.Shared.Constants;

public static class ApiConstants
{
    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string RequestId = "X-Request-ID";
    }

    public static class Policies
    {
        public const string DefaultCors = "DefaultCorsPolicy";
    }

    public static class Routes
    {
        public const string ApiVersion = "v1";
        public const string ApiBase = $"api/{ApiVersion}";
    }
}
