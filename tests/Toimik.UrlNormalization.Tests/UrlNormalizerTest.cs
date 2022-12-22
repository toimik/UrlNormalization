namespace Toimik.UrlNormalization.Tests;

using System;
using Xunit;

public class UrlNormalizerTest
{
    [Theory]
    [InlineData("file://example/%7Efoo", "file://example/~foo")]
    [InlineData("ftp://example.com/%7Efoo", "ftp://example.com/~foo")]
    public void DecodeUnreservedPercentEncodedTriplets(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData(false, "file://example/foo//bar.html", "file://example/foo//bar.html")]
    [InlineData(true, "file://example/foo//bar.html", "file://example/foo/bar.html")]
    [InlineData(false, "ftp://example.com/foo//bar.html", "ftp://example.com/foo//bar.html")]
    [InlineData(true, "ftp://example.com/foo//bar.html", "ftp://example.com/foo/bar.html")]
    public void DuplicateSlashes(
        bool isAdjacentSlashesCollapsed,
        string url,
        string expectedUrl)
    {
        var normalizer = new UrlNormalizer(isAdjacentSlashesCollapsed: isAdjacentSlashesCollapsed);

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData("file://", "file:///")]
    [InlineData("file:///foo.html", "file:///foo.html")]
    public void EmptyAuthorityThatIsNotRequired(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData("ftp://")]
    [InlineData("ftp:///foo.html")]
    public void EmptyAuthorityThatIsRequired(string url)
    {
        var normalizer = new UrlNormalizer();

        Assert.Throws<UriFormatException>(() => normalizer.Normalize(url));
    }

    [Theory]
    [InlineData("file://example", "file://example/")]
    [InlineData("ftp://example.com", "ftp://example.com/")]
    public void EmptyPath(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData("FILE://Example/Foo", "file://example/Foo")]
    [InlineData("FTP://User@Example.COM/Foo", "ftp://User@example.com/Foo")]
    public void LowercaseSchemeAndHost(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData("file://example")]
    [InlineData("ftp://example.com")]
    public void Relative(string baseUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url: "/foo.html", baseUrl);

        Assert.Equal($"{baseUrl}/foo.html", actualUrl);
    }

    [Fact]
    public void RemoveDefaultPort()
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize("ftp://example.com:21/");

        Assert.Equal("ftp://example.com/", actualUrl);
    }

    [Theory]
    [InlineData("file://example/foo/./bar/baz/../qux", "file://example/foo/bar/qux")]
    [InlineData("ftp://example.com/foo/./bar/baz/../qux", "ftp://example.com/foo/bar/qux")]
    public void RemoveDotSegments(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Fact]
    public void UnspecifiedSchemeWithBaseUrl()
    {
        var normalizer = new UrlNormalizer();

        Assert.Throws<UriFormatException>(() => normalizer.Normalize("/foo", baseUrl: "example.com"));
    }

    [Fact]
    public void UnspecifiedSchemeWithoutBaseUrl()
    {
        var normalizer = new UrlNormalizer();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize("example.com"));
    }

    [Theory]
    [InlineData("file://example/foo%2a", "file://example/foo%2A")]
    [InlineData("file://example/foo%a", "file://example/foo%25A")]
    [InlineData("ftp://example.com/foo%2a", "ftp://example.com/foo%2A")]
    [InlineData("ftp://example.com/foo%a", "ftp://example.com/foo%25A")]
    public void UppercasePercentEncodedTriplets(string url, string expectedUrl)
    {
        var normalizer = new UrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }
}