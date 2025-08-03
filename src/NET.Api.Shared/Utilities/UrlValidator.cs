namespace NET.Api.Shared.Utilities;

/// <summary>
/// Utilidad para validación de URLs
/// </summary>
public static class UrlValidator
{
    /// <summary>
    /// Valida si una URL es válida y usa esquema HTTP o HTTPS
    /// </summary>
    /// <param name="url">La URL a validar</param>
    /// <returns>True si la URL es válida, false en caso contrario</returns>
    public static bool IsValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}