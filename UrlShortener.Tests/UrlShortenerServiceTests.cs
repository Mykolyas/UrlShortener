using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Models.Results;
using UrlShortener.Services;
using Xunit;

namespace UrlShortener.Tests;

public class UrlShortenerServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        var context = new ApplicationDbContext(options);
        
        context.Database.EnsureCreated();
        
        return context;
    }

    [Fact]
    public async Task CreateShortUrlAsync_ValidUrl_ReturnsShortUrl()
    {
        var context = GetInMemoryDbContext();
        
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";

        var result = await service.CreateShortUrlAsync(originalUrl, user.Id);

        Assert.True(result.Succeeded);
        var created = result.ShortUrl;
        Assert.NotNull(created);
        Assert.Equal(originalUrl, created!.OriginalUrl);
        Assert.NotEmpty(created.ShortCode);
        Assert.Equal(6, created.ShortCode.Length);
        Assert.Equal(user.Id, created.CreatedById);
    }

    [Fact]
    public async Task CreateShortUrlAsync_DuplicateUrl_ReturnsNull()
    {
        var context = GetInMemoryDbContext();
        
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";

        var firstResult = await service.CreateShortUrlAsync(originalUrl, user.Id);
        var secondResult = await service.CreateShortUrlAsync(originalUrl, user.Id);

        Assert.True(firstResult.Succeeded);
        Assert.False(secondResult.Succeeded);
        Assert.Equal(ShortUrlCreationError.DuplicateUrl, secondResult.Error);
    }

    [Fact]
    public async Task GetByShortCodeAsync_ExistingCode_ReturnsShortUrl()
    {
        var context = GetInMemoryDbContext();
        
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var createdResult = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.True(createdResult.Succeeded);
        var created = createdResult.ShortUrl;
        Assert.NotNull(created);
        Assert.NotNull(created!.ShortCode);

        var result = await service.GetByShortCodeAsync(created.ShortCode);

        Assert.NotNull(result);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.Equal(created.ShortCode, result.ShortCode);
    }

    [Fact]
    public async Task GetByShortCodeAsync_NonExistingCode_ReturnsNull()
    {
        var context = GetInMemoryDbContext();
        var service = new UrlShortenerService(context);

        var result = await service.GetByShortCodeAsync("INVALID");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUrlAsync_OwnUrl_ReturnsTrue()
    {
        var context = GetInMemoryDbContext();
        
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var createdResult = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.True(createdResult.Succeeded);
        var created = createdResult.ShortUrl;
        Assert.NotNull(created);

        var result = await service.DeleteUrlAsync(created.Id, user.Id, isAdmin: false);

        Assert.True(result);
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteUrlAsync_OtherUsersUrl_NonAdmin_ReturnsFalse()
    {
        var context = GetInMemoryDbContext();
        
        var user1 = new ApplicationUser { Id = "test-user-1", UserName = "user1" };
        var user2 = new ApplicationUser { Id = "test-user-2", UserName = "user2" };
        context.Users.Add(user1);
        context.Users.Add(user2);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var createdResult = await service.CreateShortUrlAsync(originalUrl, user1.Id);
        Assert.True(createdResult.Succeeded);
        var created = createdResult.ShortUrl;
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        var result = await service.DeleteUrlAsync(created.Id, user2.Id, isAdmin: false);

        Assert.False(result);
        var stillExists = await service.GetByIdAsync(created.Id);
        Assert.NotNull(stillExists);
    }

    [Fact]
    public async Task DeleteUrlAsync_AnyUrl_Admin_ReturnsTrue()
    {
        var context = GetInMemoryDbContext();
        
        var user1 = new ApplicationUser { Id = "test-user-1", UserName = "user1" };
        var user2 = new ApplicationUser { Id = "test-user-2", UserName = "user2" };
        context.Users.Add(user1);
        context.Users.Add(user2);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var createdResult = await service.CreateShortUrlAsync(originalUrl, user1.Id);
        Assert.True(createdResult.Succeeded);
        var created = createdResult.ShortUrl;
        Assert.NotNull(created);

        var result = await service.DeleteUrlAsync(created!.Id, user2.Id, isAdmin: true);

        Assert.True(result);
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task IncrementClickCountAsync_ExistingCode_IncrementsCount()
    {
        var context = GetInMemoryDbContext();
        
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var createdResult = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.True(createdResult.Succeeded);
        var created = createdResult.ShortUrl;
        Assert.NotNull(created);
        Assert.NotNull(created!.ShortCode);
        var initialCount = created.ClickCount;

        await service.IncrementClickCountAsync(created.ShortCode);

        var updated = await service.GetByShortCodeAsync(created.ShortCode);
        Assert.NotNull(updated);
        Assert.Equal(initialCount + 1, updated.ClickCount);
    }
}

