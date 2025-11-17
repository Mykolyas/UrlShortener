using UrlShortener.Models;
using UrlShortener.Models.Results;

namespace UrlShortener.Services;

public interface IUrlShortenerService
{
    Task<ShortUrlCreationResult> CreateShortUrlAsync(string originalUrl, string userId);
    Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
    Task<List<ShortUrl>> GetAllUrlsAsync();
    Task<bool> DeleteUrlAsync(int id, string userId, bool isAdmin);
    Task<ShortUrl?> GetByIdAsync(int id);
    Task IncrementClickCountAsync(string shortCode);
    Task<bool> IsUrlValidForRedirect(string url);
}

