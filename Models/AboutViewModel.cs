using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models;

public class AboutViewModel
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}

