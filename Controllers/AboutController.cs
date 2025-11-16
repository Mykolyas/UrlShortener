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
<p>In this project, the URL Shortener is powered by a <strong>Base62</strong>-based algorithm that generates short, unique codes for every URL.</p>

<h3>How it works:</h3>
<ol>
    <li><strong>Input Validation:</strong> When a user submits a link, the system first checks whether it's a valid URL format.</li>
    <li><strong>Duplicate Check:</strong> Before creating anything new, the system checks whether the URL already exists in the database; otherwise, the user gets an error.</li>
    <li><strong>Short Code Generation:</strong> For a new entry, the application creates a 6-character code using Base62.</li>
    <li><strong>Uniqueness Verification:</strong> If the generated code somehow already exists, a new one is generated until a unique code is found.</li>
    <li><strong>Saving the Data:</strong> The original URL, the short code, creator info, and creation timestamp are all stored in the database.</li>
    <li><strong>Redirection:</strong> When someone opens a short URL, the app looks up the original link, redirects the user, and increments the click counter.</li>
</ol>

<h3>Technical Details:</h3>
<ul>
    <li>Each short code is exactly 6 characters long.</li>
    <li>Base62 includes the characters: 0–9, A–Z, and a–z.</li>
    <li>Every URL must be unique — duplicates are not allowed.</li>
    <li>Click tracking is implemented for monitoring usage.</li>
    <li>Real-time updates are handled through React for a smoother user experience.</li>
</ul>

<h3>Security Features:</h3>
<ul>
    <li>Authentication is required to create URLs, view details, or delete entries.</li>
    <li>Users can only delete URLs they personally created.</li>
    <li>Administrators have full access to all records.</li>
    <li>Anonymous users can view the table and use any short links.</li>
</ul>";
    }
}
