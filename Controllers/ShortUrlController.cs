using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Models;
using UrlShortener.Services;
using UrlShortener.Extensions;
using UrlShortener.Models.Dto;
using UrlShortener.Models.ViewModels;

namespace UrlShortener.Controllers;

public class ShortUrlController : Controller
{
    private readonly IUrlShortenerService _urlShortenerService;

    public ShortUrlController(IUrlShortenerService urlShortenerService)
    {
        _urlShortenerService = urlShortenerService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var urls = await _urlShortenerService.GetAllUrlsAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var dto = urls
            .Select(u => u.ToSummaryDto(baseUrl))
            .ToList()
            .AsReadOnly();

        return View(dto);
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
        var viewModel = url.ToDetailsViewModel(baseUrl);

        return View(viewModel);
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

        var isValid = await _urlShortenerService.IsUrlValidForRedirect(url.OriginalUrl);
        if (!isValid)
        {
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
            ViewBag.ErrorMessage = "This shortened URL contains invalid characters and cannot be redirected. Please delete this URL and create a new one with a valid URL.";
            ViewBag.ShortCode = shortCode;
            ViewBag.UrlId = url.Id;
            return View("InvalidUrl");
        }
    }
}
