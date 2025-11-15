using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Controllers;

public class AboutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AboutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var content = await GetAboutContentAsync();
        var viewModel = new AboutViewModel
        {
            Content = content,
            IsAdmin = User.IsInRole("Admin")
        };
        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateContent(AboutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.IsAdmin = User.IsInRole("Admin");
            return View("Index", model);
        }

        var user = await _userManager.GetUserAsync(User);
        var aboutContent = await _context.AboutContents.FirstOrDefaultAsync();

        if (aboutContent == null)
        {
            aboutContent = new AboutContent
            {
                // Id will be generated automatically by the database
                Content = model.Content,
                UpdatedBy = user?.UserName ?? "Admin",
                LastUpdated = DateTime.UtcNow
            };
            _context.AboutContents.Add(aboutContent);
        }
        else
        {
            aboutContent.Content = model.Content;
            aboutContent.UpdatedBy = user?.UserName ?? "Admin";
            aboutContent.LastUpdated = DateTime.UtcNow;
            // Mark as modified to ensure EF tracks the changes
            _context.AboutContents.Update(aboutContent);
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "Content updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GetAboutContentAsync()
    {
        var aboutContent = await _context.AboutContents.FirstOrDefaultAsync();
        
        if (aboutContent != null && !string.IsNullOrEmpty(aboutContent.Content))
        {
            return aboutContent.Content;
        }

        // Default content explaining the URL shortening algorithm
        return @"<h2>URL Shortener Algorithm</h2>
<p>This URL Shortener application uses a <strong>Base62 encoding</strong> algorithm to generate short codes for URLs.</p>

<h3>How it works:</h3>
<ol>
    <li><strong>Input Validation:</strong> When a URL is submitted, the system validates that it's a proper URL format.</li>
    <li><strong>Duplicate Check:</strong> The system checks if the URL already exists in the database. If it does, an error is returned.</li>
    <li><strong>Short Code Generation:</strong> A unique 6-character short code is generated using Base62 encoding (0-9, A-Z, a-z).</li>
    <li><strong>Uniqueness Verification:</strong> The system ensures the generated code doesn't already exist in the database. If it does, a new code is generated.</li>
    <li><strong>Storage:</strong> The original URL, short code, creator information, and timestamp are stored in the database.</li>
    <li><strong>Redirection:</strong> When someone accesses the short URL (r/{shortCode}), the system looks up the original URL and redirects them, while incrementing the click count.</li>
</ol>

<h3>Technical Details:</h3>
<ul>
    <li>Short codes are 6 characters long, providing 62^6 = 56,800,235,584 possible combinations</li>
    <li>Base62 encoding uses characters: 0-9, A-Z, a-z</li>
    <li>Each URL is unique - duplicate URLs return an error message</li>
    <li>Click tracking is implemented to monitor usage</li>
    <li>Real-time updates using React for a seamless user experience</li>
</ul>

<h3>Security Features:</h3>
<ul>
    <li>Authentication required for creating, viewing details, and deleting URLs</li>
    <li>Users can only delete URLs they created</li>
    <li>Administrators have full access to all URLs</li>
    <li>Anonymous users can view the table and use shortened URLs</li>
</ul>";
    }
}
