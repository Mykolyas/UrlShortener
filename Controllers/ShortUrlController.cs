using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UrlShortener.Models;
using UrlShortener.Services;

namespace UrlShortener.Controllers;

public class ShortUrlController : Controller
{
    private readonly IUrlShortenerService _urlShortenerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ShortUrlController(
        IUrlShortenerService urlShortenerService,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _urlShortenerService = urlShortenerService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var urls = await _urlShortenerService.GetAllUrlsAsync();
        return View(urls);
    }

    [HttpPost]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateShortUrl([FromBody] CreateShortUrlRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.OriginalUrl))
        {
            return Json(new { success = false, message = "Invalid URL format. Please enter a valid URL (e.g., https://example.com)" });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var shortUrl = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl, user.Id);
        
        if (shortUrl == null)
        {
            // Check if URL already exists by trying to find it
            var allUrls = await _urlShortenerService.GetAllUrlsAsync();
            var exists = allUrls.Any(u => u.OriginalUrl == request.OriginalUrl);
            
            if (exists)
            {
                return Json(new { success = false, message = "This URL already exists." });
            }
            else
            {
                return Json(new { success = false, message = "Invalid URL. URL contains invalid characters or is not properly formatted. Please use only ASCII characters in the URL (e.g., https://example.com)." });
            }
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Json(new
        {
            success = true,
            data = new
            {
                id = shortUrl.Id,
                originalUrl = shortUrl.OriginalUrl,
                shortCode = shortUrl.ShortCode,
                shortUrl = $"{baseUrl}/r/{shortUrl.ShortCode}",
                createdDate = shortUrl.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                createdBy = user.UserName ?? string.Empty,
                clickCount = shortUrl.ClickCount
            }
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var result = await _urlShortenerService.DeleteUrlAsync(id, user.Id, isAdmin);

        if (!result)
        {
            return Json(new { success = false, message = "Unable to delete URL. You may not have permission." });
        }

        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Info(int id)
    {
        var url = await _urlShortenerService.GetByIdAsync(id);
        if (url == null)
        {
            return NotFound();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        ViewBag.ShortUrl = $"{baseUrl}/r/{url.ShortCode}";
        
        return View(url);
    }

    [HttpGet]
    [Route("r/{shortCode}")]
    public new async Task<IActionResult> Redirect(string shortCode)
    {
        var url = await _urlShortenerService.GetByShortCodeAsync(shortCode);
        if (url == null)
        {
            return NotFound();
        }

        // Validate URL before redirect
        var isValid = await _urlShortenerService.IsUrlValidForRedirect(url.OriginalUrl);
        if (!isValid)
        {
            // Mark URL as invalid and show error page
            ViewBag.ErrorMessage = "This shortened URL contains invalid characters and cannot be redirected. Please delete this URL and create a new one with a valid URL.";
            ViewBag.ShortCode = shortCode;
            ViewBag.UrlId = url.Id;
            return View("InvalidUrl");
        }

        await _urlShortenerService.IncrementClickCountAsync(shortCode);
        
        try
        {
            return base.Redirect(url.OriginalUrl);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Invalid non-ASCII") || ex.Message.Contains("header"))
        {
            // Handle the specific error about invalid characters in header
            ViewBag.ErrorMessage = "This shortened URL contains invalid characters and cannot be redirected. Please delete this URL and create a new one with a valid URL.";
            ViewBag.ShortCode = shortCode;
            ViewBag.UrlId = url.Id;
            return View("InvalidUrl");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUrls()
    {
        var urls = await _urlShortenerService.GetAllUrlsAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = urls.Select(u => new
        {
            id = u.Id,
            originalUrl = u.OriginalUrl,
            shortCode = u.ShortCode,
            shortUrl = $"{baseUrl}/r/{u.ShortCode}",
            createdDate = u.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            createdBy = u.CreatedBy?.UserName ?? string.Empty,
            clickCount = u.ClickCount
        }).ToList();

        return Json(result);
    }
}

public class CreateShortUrlRequest
{
    [Required]
    [Url]
    public string OriginalUrl { get; set; } = string.Empty;
}

