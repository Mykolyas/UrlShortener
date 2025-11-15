using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}

