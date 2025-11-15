using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;
using UrlShortener.Controllers;
using UrlShortener.Models;
using UrlShortener.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortUrlControllerTests
{
    [Fact]
    public async Task CreateShortUrl_ValidUrl_ReturnsSuccess()
    {
        // Arrange
        var mockService = new Mock<IUrlShortenerService>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var controller = new ShortUrlController(
            mockService.Object,
            mockUserManager.Object,
            mockHttpContextAccessor.Object);

        // Setup HttpContext for Request
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.ControllerContext.HttpContext = httpContext;

        var user = new ApplicationUser { Id = "user-1", UserName = "testuser" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var shortUrl = new ShortUrl
        {
            Id = 1,
            OriginalUrl = "https://example.com",
            ShortCode = "ABC123",
            CreatedById = user.Id,
            CreatedDate = DateTime.UtcNow
        };

        mockService.Setup(x => x.CreateShortUrlAsync("https://example.com", user.Id))
            .ReturnsAsync(shortUrl);

        // Mock GetAllUrlsAsync to return empty list (for duplicate check)
        mockService.Setup(x => x.GetAllUrlsAsync())
            .ReturnsAsync(new List<ShortUrl>());

        var request = new CreateShortUrlRequest { OriginalUrl = "https://example.com" };

        // Act
        var result = await controller.CreateShortUrl(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var response = jsonResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task CreateShortUrl_DuplicateUrl_ReturnsError()
    {
        // Arrange
        var mockService = new Mock<IUrlShortenerService>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var controller = new ShortUrlController(
            mockService.Object,
            mockUserManager.Object,
            mockHttpContextAccessor.Object);

        var user = new ApplicationUser { Id = "user-1", UserName = "testuser" };
        mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        mockService.Setup(x => x.CreateShortUrlAsync("https://example.com", user.Id))
            .ReturnsAsync((ShortUrl?)null); // Duplicate URL returns null

        // Mock GetAllUrlsAsync to return a list with the duplicate URL
        var existingUrl = new ShortUrl
        {
            Id = 1,
            OriginalUrl = "https://example.com",
            ShortCode = "EXIST1",
            CreatedById = "other-user",
            CreatedDate = DateTime.UtcNow
        };
        mockService.Setup(x => x.GetAllUrlsAsync())
            .ReturnsAsync(new List<ShortUrl> { existingUrl });

        var request = new CreateShortUrlRequest { OriginalUrl = "https://example.com" };

        // Act
        var result = await controller.CreateShortUrl(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var response = jsonResult.Value;
        Assert.NotNull(response);
        
        // Verify it returns error message about duplicate
        var responseDict = response as dynamic;
        if (responseDict != null)
        {
            var success = responseDict.GetType().GetProperty("success")?.GetValue(responseDict);
            Assert.False((bool?)success ?? true);
        }
    }
}

