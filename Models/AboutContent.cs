namespace UrlShortener.Models;

public class AboutContent
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public string UpdatedBy { get; set; } = string.Empty;
}

