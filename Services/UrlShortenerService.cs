using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

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

    public async Task<ShortUrl?> CreateShortUrlAsync(string originalUrl, string userId)
    {
        // Validate URL format and characters
        if (!IsValidUrl(originalUrl))
        {
            return null; // Invalid URL
        }

        // Check if URL already exists
        var existingUrl = await _context.ShortUrls
            .FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);

        if (existingUrl != null)
        {
            return null; // URL already exists
        }

        // Generate unique short code
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

        return shortUrl;
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

        // Check permissions: admin can delete all, users can only delete their own
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
        // Generate random short code using Base62 encoding
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

        // Try to parse as URI
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        // Check if scheme is http or https
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        // Check if URL contains only ASCII characters (or properly encoded)
        // This prevents issues with HTTP headers
        try
        {
            // Try to encode the URL - if it contains invalid characters, this will fail
            var encoded = Uri.EscapeUriString(url);
            
            // Check if the URL can be used in HTTP Location header
            // Location header must contain only ASCII characters
            foreach (char c in url)
            {
                // Allow ASCII printable characters and common URL characters
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
        // Allow some common non-ASCII characters that are typically URL-encoded
        // But in practice, we should require proper URL encoding
        return false; // For safety, require ASCII only or proper encoding
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

            // Check if URL can be used in HTTP Location header
            // Location header must be ASCII
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

