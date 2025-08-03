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
        
        // Políticas de autorización por rol
        public const string RequireOwnerRole = "RequireOwnerRole";
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireModeratorRole = "RequireModeratorRole";
        public const string RequireSupportRole = "RequireSupportRole";
        public const string RequireUserRole = "RequireUserRole";
        
        // Políticas de autorización por jerarquía
        public const string RequireElevatedRoles = "RequireElevatedRoles";
        public const string RequireAdminOrAbove = "RequireAdminOrAbove";
        public const string RequireModeratorOrAbove = "RequireModeratorOrAbove";
        
        // Políticas específicas de funcionalidad
        public const string CanManageUsers = "CanManageUsers";
        public const string CanManageRoles = "CanManageRoles";
        public const string CanViewReports = "CanViewReports";
        public const string CanModerateContent = "CanModerateContent";
        public const string CanAccessSupport = "CanAccessSupport";
    }

    public static class Routes
    {
        public const string ApiVersion = "v1";
        public const string ApiBase = $"api/{ApiVersion}";
    }
}
