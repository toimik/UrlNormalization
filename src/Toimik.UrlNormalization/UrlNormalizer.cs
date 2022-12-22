/*
 * Copyright 2021-2022 nurhafiz@hotmail.sg
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Toimik.UrlNormalization;

using System;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a normalizer that standardizes different representations of an absolute URL.
/// </summary>
public class UrlNormalizer
{
    private static readonly Regex AdjacentSlashesRegex = new("/{2,}", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlNormalizer"/> class with optional
    /// configurations.
    /// </summary>
    /// <param name="isAdjacentSlashesCollapsed">
    /// If <c>true</c>, occurrences of two or more adjacent slashes in a path is collapsed into
    /// one. The default is <c>true</c> .
    /// </param>
    public UrlNormalizer(bool isAdjacentSlashesCollapsed = true)
    {
        IsAdjacentSlashesCollapsed = isAdjacentSlashesCollapsed;
    }

    public bool IsAdjacentSlashesCollapsed { get; }

    /// <summary>
    /// Normalizes a URL.
    /// </summary>
    /// <remarks>
    /// In addition to what <see cref="Uri"/> does by default (lowercasing the scheme and host,
    /// removing of default port (80) and dot segments, decoding percent-encoded triplets of
    /// unreserved characters), the following are standardized:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// User info (username:password@) component, if any, is removed if
    /// <paramref name="IsUserInfoIgnored"/> is <c>true</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>Path, if none, defaults to '/'.</description>
    /// </item>
    /// <item>
    /// <description>
    /// Path, if any, with multiple adjacent slashes are replaced with '/' if
    /// <see cref="IsAdjacentSlashesCollapsed"/> is <c>true</c>.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="url">
    /// A relative or absolute URL.
    /// </param>
    /// <param name="baseUrl">
    /// The base URL to prefix <paramref name="url"/> with if the latter is relative. The
    /// default is <c>null</c> .
    /// </param>
    /// <returns>
    /// A normalized representation of the parameter(s).
    /// </returns>
    /// <exception cref="UriFormatException">
    /// Thrown if scheme is unspecified, or authority is unspecified if scheme is FTP.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="url"/> does not have a scheme and <paramref name="baseUrl"/>
    /// is <c>null</c>.
    /// </exception>
    public virtual string Normalize(string url, string? baseUrl = null)
    {
        url = url.Trim();

        // Check if scheme is specified
        string scheme;
        var schemeDelimiter = Uri.SchemeDelimiter;
        int schemeDelimiterIndex;
        var tryCount = 0;
        do
        {
            schemeDelimiterIndex = url.IndexOf(schemeDelimiter);
            if (schemeDelimiterIndex != -1)
            {
                break;
            }

            if (tryCount == 1)
            {
                throw new UriFormatException("Scheme is unspecified.");
            }

            tryCount++;
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            // Prefix the url with the base URL as it is determined that the former is relative.
            // A slash is added between them in case the url is not prefixed with one. Multiple
            // slashes are automatically removed down the line.
            url = $"{baseUrl}/{url}";
        }
        while (true);

        scheme = url[..schemeDelimiterIndex];
        url = url[(schemeDelimiterIndex + schemeDelimiter.Length)..];

        // Determine the authority, if any
        string authority;
        var slashIndex = url.IndexOf('/');
        if (slashIndex == -1)
        {
            authority = url;
            url = string.Empty;
        }
        else
        {
            authority = url[..slashIndex];
            url = url[slashIndex..];
        }

        // Determine the path, if any
        var path = url;
        if (path == string.Empty)
        {
            path = "/";
        }
        else
        {
            path = IsAdjacentSlashesCollapsed
                ? AdjacentSlashesRegex.Replace(path, "/")
                : path;
            path = NormalizePath(path);
        }

        url = CreateUrl(
            scheme,
            authority,
            path);
        return url;
    }

    protected virtual string CreateUrl(
        string scheme,
        string authority,
        string path)
    {
        // Create an absolute Uri where all the other normalizations are done by default
        var url = $"{scheme}{Uri.SchemeDelimiter}{authority}{path}";
        var uri = new Uri(url, UriKind.Absolute);
        return uri.AbsoluteUri;
    }

    protected virtual string NormalizePath(string path)
    {
        path = NormalizePercentEncoding(path);
        return path;
    }

    protected virtual string NormalizePercentEncoding(string text)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            var character = text[i];
            builder.Append(character);
            if (character.Equals('%'))
            {
                for (int j = 1; j <= 2; j++)
                {
                    i++;
                    if (i == text.Length)
                    {
                        // As the encoding is at the end of the URL and is malformed, the value
                        // is auto decoded by Uri class
                        break;
                    }

                    character = text[i];
                    builder.Append(char.ToUpper(character));
                }
            }
        }

        text = builder.ToString();
        return text;
    }
}