using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using UrlShortener.Controllers.Api;
using UrlShortener.Models;
using UrlShortener.Models.Dto;
using UrlShortener.Models.Results;
using UrlShortener.Services;
using Xunit;

namespace UrlShortener.Tests;

public class ShortUrlsApiControllerTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsPayload()
    {
        var mockService = new Mock<IUrlShortenerService>();
        var mockUserManager = CreateUserManagerMock();

        var controller = new ShortUrlsApiController(mockService.Object, mockUserManager.Object)
        {
            ControllerContext = BuildControllerContext()
        };

        var user = new ApplicationUser { Id = "user-1", UserName = "tester" };
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

        mockService.Setup(x => x.CreateShortUrlAsync(shortUrl.OriginalUrl, user.Id))
            .ReturnsAsync(ShortUrlCreationResult.Success(shortUrl));

        var request = new CreateShortUrlRequestDto { OriginalUrl = shortUrl.OriginalUrl };

        var actionResult = await controller.CreateAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var payload = Assert.IsType<ShortUrlSummaryDto>(okResult.Value);
        Assert.Equal(shortUrl.ShortCode, payload.ShortCode);
        Assert.Equal(shortUrl.OriginalUrl, payload.OriginalUrl);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUrl_ReturnsConflict()
    {
        var mockService = new Mock<IUrlShortenerService>();
        var mockUserManager = CreateUserManagerMock();

        var controller = new ShortUrlsApiController(mockService.Object, mockUserManager.Object)
        {
            ControllerContext = BuildControllerContext()
        };

        var user = new ApplicationUser { Id = "user-1", UserName = "tester" };
        mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        mockService.Setup(x => x.CreateShortUrlAsync("https://example.com", user.Id))
            .ReturnsAsync(ShortUrlCreationResult.Duplicate("https://example.com"));

        var request = new CreateShortUrlRequestDto { OriginalUrl = "https://example.com" };

        var actionResult = await controller.CreateAsync(request);

        Assert.IsType<ConflictObjectResult>(actionResult);
    }

    private static ControllerContext BuildControllerContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }
}

