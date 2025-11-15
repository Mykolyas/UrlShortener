using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models;

public class ShortUrl
{
    public int Id { get; set; }

    [Required]
    [Url]
    public string OriginalUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string ShortCode { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public virtual ApplicationUser CreatedBy { get; set; } = null!;

    public int ClickCount { get; set; } = 0;
}

