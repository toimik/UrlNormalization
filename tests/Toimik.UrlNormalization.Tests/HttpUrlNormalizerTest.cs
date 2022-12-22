namespace Toimik.UrlNormalization.Tests;

using System;
using System.Collections.Generic;
using Xunit;

public class HttpUrlNormalizerTest
{
    [Theory]
    [InlineData(false, "http://example.com/default.asp", "http://example.com/default.asp")]
    [InlineData(false, "http://example.com/a/index.html", "http://example.com/a/index.html")]
    [InlineData(true, "http://example.com/default.asp", "http://example.com/")]
    [InlineData(true, "http://example.com/a/index.html", "http://example.com/a/")]
    [InlineData(null, "http://example.com/home.aspx", "http://example.com/")]
    public void DirectoryIndex(
        bool? isDirectoryIndexRemoved,
        string url,
        string expectedUrl)
    {
        ISet<string> removableDirectoryIndexNames;
        if (isDirectoryIndexRemoved == null)
        {
            removableDirectoryIndexNames = new HashSet<string>
            {
                "home",
            };
        }
        else
        {
            removableDirectoryIndexNames = isDirectoryIndexRemoved.Value
                ? HttpUrlNormalizer.DefaultRemovableDirectoryIndexNames // or null (the former is used to ensure that the variable is public)
                : new HashSet<string>(0);
        }

        var normalizer = new HttpUrlNormalizer(removableDirectoryIndexNames: removableDirectoryIndexNames);

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData(true, "http://example.com/bar.html#section1", "http://example.com/bar.html")]
    [InlineData(false, "http://example.com/bar.html#", "http://example.com/bar.html")]
    [InlineData(false, "http://example.com/bar.html#section1", "http://example.com/bar.html#section1")]
    [InlineData(false, "http://example.com/bar.html?#section1", "http://example.com/bar.html#section1")]
    public void Fragment(
        bool isFragmentIgnored,
        string url,
        string expectedUrl)
    {
        var normalizer = new HttpUrlNormalizer(isFragmentIgnored: isFragmentIgnored);

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData("http://example.com/?#", "http://example.com/")]
    [InlineData("http://example.com/display?=", "http://example.com/display")]
    [InlineData("http://example.com/display?text", "http://example.com/display?text")]
    [InlineData("http://example.com/display?key=", "http://example.com/display?key=")]
    [InlineData("http://example.com/display?=value", "http://example.com/display")]
    [InlineData("http://example.com/display?key=&key=", "http://example.com/display?key=")]
    [InlineData("http://example.com/display?b=dog&a=animals&b=cat&=_", "http://example.com/display?a=animals&b=cat&b=dog")]
    public void QueryString(string url, string expectedUrl)
    {
        var normalizer = new HttpUrlNormalizer();

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Fact]
    public void SchemeThatIsCustomized()
    {
        var normalizer = new HttpUrlNormalizer();

        Assert.Throws<UriFormatException>(() => normalizer.Normalize("invalid://example.com"));
    }

    [Fact]
    public void SchemeThatIsInvalid()
    {
        var normalizer = new HttpUrlNormalizer();

        Assert.Throws<UriFormatException>(() => normalizer.Normalize("invalid://example.com"));
    }

    [Theory]
    [InlineData("http://www.example.com", "http", "http://www.example.com/")]
    [InlineData("https://www.example.com", "http", "http://www.example.com/")]
    [InlineData("http://www.example.com", "https", "https://www.example.com/")]
    [InlineData("https://www.example.com", "https", "https://www.example.com/")]
    public void SchemeThatIsPreferred(
        string url,
        string preferredScheme,
        string expectedUrl)
    {
        var normalizer = new HttpUrlNormalizer(preferredScheme: preferredScheme);

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Theory]
    [InlineData(true, "http://:@example.com", "http://example.com/")]
    [InlineData(true, "http://user@example.com", "http://example.com/")]
    [InlineData(true, "http://:password@example.com", "http://example.com/")]
    [InlineData(true, "http://username:password@example.com", "http://example.com/")]
    [InlineData(false, "http://:@example.com", "http://:@example.com/")]
    [InlineData(false, "http://user@example.com", "http://user@example.com/")]
    [InlineData(false, "http://:password@example.com", "http://:password@example.com/")]
    [InlineData(false, "http://username:password@example.com", "http://username:password@example.com/")]
    public void UserInfo(
        bool isUserInfoIgnored,
        string url,
        string expectedUrl)
    {
        var normalizer = new HttpUrlNormalizer(isUserInfoIgnored: isUserInfoIgnored);

        var actualUrl = normalizer.Normalize(url);

        Assert.Equal(expectedUrl, actualUrl);
    }
}