namespace UrlShortener.Models.Dto;

/// <summary>
/// Response model returned to clients (React SPA) for short URL list/details.
/// </summary>
public class ShortUrlSummaryDto
{
    public int Id { get; init; }
    public string OriginalUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public string CreatedDate { get; init; } = string.Empty;
    public int ClickCount { get; init; }
}


