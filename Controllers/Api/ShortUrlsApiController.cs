using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Extensions;
using UrlShortener.Models;
using UrlShortener.Models.Dto;
using UrlShortener.Models.Results;
using UrlShortener.Services;

namespace UrlShortener.Controllers.Api;

[ApiController]
[Route("api/short-urls")]
public class ShortUrlsApiController : ControllerBase
{
    private readonly IUrlShortenerService _urlShortenerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShortUrlsApiController(
        IUrlShortenerService urlShortenerService,
        UserManager<ApplicationUser> userManager)
    {
        _urlShortenerService = urlShortenerService;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ShortUrlSummaryDto>>> GetAllAsync()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var urls = await _urlShortenerService.GetAllUrlsAsync();
        var payload = urls.Select(u => u.ToSummaryDto(baseUrl));
        return Ok(payload);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAsync([FromBody] CreateShortUrlRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User context not found." });
        }

        var creationResult = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl, user.Id);
        if (!creationResult.Succeeded)
        {
            return creationResult.Error switch
            {
                ShortUrlCreationError.InvalidUrlFormat => BadRequest(new { message = "Invalid URL format. Please submit a valid absolute URL (https://example.com)." }),
                ShortUrlCreationError.DuplicateUrl => Conflict(new { message = "This URL already exists." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unable to create short URL." })
            };
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var dto = creationResult.ShortUrl!.ToSummaryDto(baseUrl, user.UserName);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "User context not found." });
        }

        var existing = await _urlShortenerService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var deleted = await _urlShortenerService.DeleteUrlAsync(id, user.Id, isAdmin);

        if (!deleted)
        {
            return Forbid();
        }

        return NoContent();
    }
}


