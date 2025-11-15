using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;
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
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        return context;
    }

    [Fact]
    public async Task CreateShortUrlAsync_ValidUrl_ReturnsShortUrl()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create a user first (required for foreign key constraint)
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";

        // Act
        var result = await service.CreateShortUrlAsync(originalUrl, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.NotEmpty(result.ShortCode);
        Assert.Equal(6, result.ShortCode.Length);
        Assert.Equal(user.Id, result.CreatedById);
    }

    [Fact]
    public async Task CreateShortUrlAsync_DuplicateUrl_ReturnsNull()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create a user first
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";

        // Act
        var firstResult = await service.CreateShortUrlAsync(originalUrl, user.Id);
        var secondResult = await service.CreateShortUrlAsync(originalUrl, user.Id);

        // Assert
        Assert.NotNull(firstResult);
        Assert.Null(secondResult);
    }

    [Fact]
    public async Task GetByShortCodeAsync_ExistingCode_ReturnsShortUrl()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create a user first (required for CreatedBy navigation property)
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var created = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.NotNull(created);
        Assert.NotNull(created.ShortCode);

        // Act
        var result = await service.GetByShortCodeAsync(created.ShortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.Equal(created.ShortCode, result.ShortCode);
    }

    [Fact]
    public async Task GetByShortCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new UrlShortenerService(context);

        // Act
        var result = await service.GetByShortCodeAsync("INVALID");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUrlAsync_OwnUrl_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create a user first
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var created = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.NotNull(created);

        // Act
        var result = await service.DeleteUrlAsync(created.Id, user.Id, isAdmin: false);

        // Assert
        Assert.True(result);
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteUrlAsync_OtherUsersUrl_NonAdmin_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create users first
        var user1 = new ApplicationUser { Id = "test-user-1", UserName = "user1" };
        var user2 = new ApplicationUser { Id = "test-user-2", UserName = "user2" };
        context.Users.Add(user1);
        context.Users.Add(user2);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var created = await service.CreateShortUrlAsync(originalUrl, user1.Id);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        // Act
        var result = await service.DeleteUrlAsync(created.Id, user2.Id, isAdmin: false);

        // Assert
        Assert.False(result);
        var stillExists = await service.GetByIdAsync(created.Id);
        Assert.NotNull(stillExists);
    }

    [Fact]
    public async Task DeleteUrlAsync_AnyUrl_Admin_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create users first
        var user1 = new ApplicationUser { Id = "test-user-1", UserName = "user1" };
        var user2 = new ApplicationUser { Id = "test-user-2", UserName = "user2" };
        context.Users.Add(user1);
        context.Users.Add(user2);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var created = await service.CreateShortUrlAsync(originalUrl, user1.Id);
        Assert.NotNull(created);

        // Act
        var result = await service.DeleteUrlAsync(created.Id, user2.Id, isAdmin: true);

        // Assert
        Assert.True(result);
        var deleted = await service.GetByIdAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task IncrementClickCountAsync_ExistingCode_IncrementsCount()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        // Create a user first
        var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var service = new UrlShortenerService(context);
        var originalUrl = "https://example.com";
        var created = await service.CreateShortUrlAsync(originalUrl, user.Id);
        Assert.NotNull(created);
        Assert.NotNull(created.ShortCode);
        var initialCount = created.ClickCount;

        // Act
        await service.IncrementClickCountAsync(created.ShortCode);

        // Assert
        var updated = await service.GetByShortCodeAsync(created.ShortCode);
        Assert.NotNull(updated);
        Assert.Equal(initialCount + 1, updated.ClickCount);
    }
}

