using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Models.Results;

namespace UrlShortener.Services;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly ApplicationDbContext _context;
    private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int ShortCodeLength = 6;

    public UrlShortenerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShortUrlCreationResult> CreateShortUrlAsync(string originalUrl, string userId)
    {
        if (!IsValidUrl(originalUrl))
        {
            return ShortUrlCreationResult.InvalidUrl(originalUrl);
        }

        var existingUrl = await _context.ShortUrls
            .FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);

        if (existingUrl != null)
        {
            return ShortUrlCreationResult.Duplicate(originalUrl);
        }

        string shortCode;
        do
        {
            shortCode = GenerateShortCode();
        } while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode));

        var shortUrl = new ShortUrl
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow
        };

        _context.ShortUrls.Add(shortUrl);
        await _context.SaveChangesAsync();

        return ShortUrlCreationResult.Success(shortUrl);
    }

    public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.ShortUrls
            .Include(u => u.CreatedBy)
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }

    public async Task<List<ShortUrl>> GetAllUrlsAsync()
    {
        return await _context.ShortUrls
            .Include(u => u.CreatedBy)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteUrlAsync(int id, string userId, bool isAdmin)
    {
        var url = await _context.ShortUrls.FindAsync(id);
        if (url == null)
        {
            return false;
        }

        if (!isAdmin && url.CreatedById != userId)
        {
            return false;
        }

        _context.ShortUrls.Remove(url);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ShortUrl?> GetByIdAsync(int id)
    {
        return await _context.ShortUrls
            .Include(u => u.CreatedBy)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task IncrementClickCountAsync(string shortCode)
    {
        var url = await _context.ShortUrls
            .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

        if (url != null)
        {
            url.ClickCount++;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateShortCode()
    {
        var random = new Random();
        var chars = new char[ShortCodeLength];

        for (int i = 0; i < ShortCodeLength; i++)
        {
            chars[i] = Base62Chars[random.Next(Base62Chars.Length)];
        }

        return new string(chars);
    }

    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        try
        {
            var encoded = Uri.EscapeUriString(url);
            
            foreach (char c in url)
            {
                if (c > 127 && !IsValidNonAsciiInUrl(c))
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidNonAsciiInUrl(char c)
    {
        return false;
    }

    public async Task<bool> IsUrlValidForRedirect(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            foreach (char c in url)
            {
                if (c > 127)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

