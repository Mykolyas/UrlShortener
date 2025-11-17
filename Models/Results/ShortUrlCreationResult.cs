using UrlShortener.Models;

namespace UrlShortener.Models.Results;

public enum ShortUrlCreationError
{
    None = 0,
    InvalidUrlFormat,
    DuplicateUrl
}

public sealed class ShortUrlCreationResult
{
    private ShortUrlCreationResult(
        bool succeeded,
        ShortUrl? shortUrl,
        ShortUrlCreationError error,
        string originalUrl)
    {
        Succeeded = succeeded;
        ShortUrl = shortUrl;
        Error = error;
        OriginalUrl = originalUrl;
    }

    public bool Succeeded { get; }
    public ShortUrl? ShortUrl { get; }
    public ShortUrlCreationError Error { get; }
    public string OriginalUrl { get; }

    public static ShortUrlCreationResult Success(ShortUrl shortUrl) =>
        new(true, shortUrl, ShortUrlCreationError.None, shortUrl.OriginalUrl);

    public static ShortUrlCreationResult InvalidUrl(string originalUrl) =>
        new(false, null, ShortUrlCreationError.InvalidUrlFormat, originalUrl);

    public static ShortUrlCreationResult Duplicate(string originalUrl) =>
        new(false, null, ShortUrlCreationError.DuplicateUrl, originalUrl);
}


