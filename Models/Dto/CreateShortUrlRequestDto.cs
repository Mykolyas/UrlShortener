using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models.Dto;

/// <summary>
/// Request model for creating a new short URL from the client application.
/// </summary>
public class CreateShortUrlRequestDto
{
    [Required]
    [Url(ErrorMessage = "Please provide a valid absolute URL (https://example.com).")]
    [MaxLength(2048, ErrorMessage = "URL too long. Please provide a shorter address.")]
    public string OriginalUrl { get; set; } = string.Empty;
}


