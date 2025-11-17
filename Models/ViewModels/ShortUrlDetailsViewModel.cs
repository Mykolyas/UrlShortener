namespace UrlShortener.Models.ViewModels;

public class ShortUrlDetailsViewModel
{
    public string OriginalUrl { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public int ClickCount { get; init; }
}


