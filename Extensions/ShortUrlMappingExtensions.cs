using UrlShortener.Models;
using UrlShortener.Models.Dto;
using UrlShortener.Models.ViewModels;

namespace UrlShortener.Extensions;

public static class ShortUrlMappingExtensions
{
    public static ShortUrlSummaryDto ToSummaryDto(
        this ShortUrl entity,
        string baseUrl,
        string? createdByOverride = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ShortUrlSummaryDto
        {
            Id = entity.Id,
            OriginalUrl = entity.OriginalUrl,
            ShortCode = entity.ShortCode,
            ShortUrl = $"{baseUrl}/r/{entity.ShortCode}",
            CreatedBy = createdByOverride ?? entity.CreatedBy?.UserName ?? string.Empty,
            CreatedDate = entity.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            ClickCount = entity.ClickCount
        };
    }

    public static ShortUrlDetailsViewModel ToDetailsViewModel(
        this ShortUrl entity,
        string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ShortUrlDetailsViewModel
        {
            OriginalUrl = entity.OriginalUrl,
            ShortUrl = $"{baseUrl}/r/{entity.ShortCode}",
            ShortCode = entity.ShortCode,
            CreatedBy = entity.CreatedBy?.UserName ?? string.Empty,
            CreatedDate = entity.CreatedDate,
            ClickCount = entity.ClickCount
        };
    }
}


