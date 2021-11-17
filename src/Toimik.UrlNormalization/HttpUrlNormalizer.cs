/*
 * Copyright 2021 nurhafiz@hotmail.sg
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

namespace Toimik.UrlNormalization
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <inheritdoc/>
    /// <remarks>
    /// This is specific to URLs with a 'http' / 'https' scheme.
    /// </remarks>
    public class HttpUrlNormalizer : UrlNormalizer
    {
        // This is public so that subclasses can use this if desired
        public static readonly ISet<string> DefaultRemovableDirectoryIndexNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "index",
            "default",
        };

        private static readonly ISet<string> Schemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "http",
            "https",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUrlNormalizer"/> class with optional
        /// configurations.
        /// </summary>
        /// <param name="isAdjacentSlashesCollapsed">
        /// See <see cref="base.UrlNormalizer(bool, bool)"/>.
        /// </param>
        /// <param name="preferredScheme">
        /// If non <c>null</c>, the scheme is changed to this (either 'http' or 'https'). If
        /// <c>null</c>, the scheme is left as-is. The default is <c>null</c>.
        /// </param>
        /// <param name="isUserInfoIgnored">
        /// If <c>true</c>, user-info (username:[password]@) component is removed. The default is
        /// <c>true</c>.
        /// </param>
        /// <param name="removableDirectoryIndexNames">
        /// Case-insensitive names (e.g. index) that must exist in a URL's filename (e.g.
        /// index.html) to indicate that the filename must be removed from the URL. The default are
        /// <c>index</c> and <c>default</c> . Pass an empty set if inapplicable.
        /// </param>
        /// <param name="isFragmentIgnored">
        /// If <c>true</c>, the fragment component, if any, of a URL is removed. The default is
        /// <c>true</c>.
        /// </param>
        public HttpUrlNormalizer(
            bool isAdjacentSlashesCollapsed = true,
            string preferredScheme = null,
            bool isUserInfoIgnored = true,
            ISet<string> removableDirectoryIndexNames = null,
            bool isFragmentIgnored = true)
            : base(isAdjacentSlashesCollapsed)
        {
            PreferredScheme = preferredScheme;
            IsUserInfoIgnored = isUserInfoIgnored;
            RemovableDirectoryIndexNames = removableDirectoryIndexNames ?? DefaultRemovableDirectoryIndexNames;
            IsFragmentIgnored = isFragmentIgnored;
        }

        public bool IsFragmentIgnored { get; }

        public bool IsUserInfoIgnored { get; }

        public string PreferredScheme { get; }

        public ISet<string> RemovableDirectoryIndexNames { get; }

        /// <inheritdoc/>
        /// <remarks>
        /// In addition to what <see cref="UrlNormalizer.Normalize(string, string)"/> does, these
        /// normalizations are performed:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Scheme is changed to <see cref="PreferredScheme"/> if the latter is non <c>null</c>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Filename, if any, is replaced with '/' if its name is in
        /// <see cref="RemovableDirectoryIndexNames"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>Path's percent encoded triplets, if any, are capitalized.</description>
        /// </item>
        /// <item>
        /// <description>Query string, if any, without parameters are removed.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Query string parameter, if any, without an equal sign remains as-is if that is the only
        /// parameter. Otherwise, an equal sign and an empty value is added.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Query string parameter, if any, with consecutive equal signs have the second one onwards
        /// treated as part of the value.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Query string parameters, if any, are sorted alphabetically by keys, and for each key, by
        /// value.
        /// </description>
        /// </item>
        /// <item>
        /// <description>Fragment, if any, without a value is removed.</description>
        /// </item>
        /// </list>
        /// <para>NOTE: Query string parameter with an empty key and / or value is allowed.</para>
        /// </remarks>
        public override string Normalize(string url, string baseUrl = null)
        {
            url = base.Normalize(url, baseUrl);

            // Determine the query, if any, and fragment, if any
            string query;
            string fragment;
            var questionMarkIndex = url.IndexOf('?');
            if (questionMarkIndex == -1)
            {
                query = string.Empty;
                var hashIndex = url.IndexOf('#');
                if (hashIndex == -1)
                {
                    fragment = string.Empty;
                }
                else
                {
                    fragment = url[hashIndex..];
                    fragment = NormalizeFragment(fragment);
                    url = url.Substring(0, hashIndex);
                }
            }
            else
            {
                var hashIndex = url.IndexOf('#', questionMarkIndex);
                if (hashIndex == -1)
                {
                    query = url[questionMarkIndex..];
                    fragment = string.Empty;
                }
                else
                {
                    var length = hashIndex - questionMarkIndex;
                    query = url.Substring(questionMarkIndex, length);
                    fragment = url[hashIndex..];
                    fragment = NormalizeFragment(fragment);
                }

                query = NormalizeQuery(query);
                url = url.Substring(0, questionMarkIndex);
            }

            url = $"{url}{query}{fragment}";
            return url;
        }

        protected override string CreateUrl(
            string scheme,
            string authority,
            string path)
        {
            if (!Schemes.Contains(scheme))
            {
                throw new UriFormatException("Invalid scheme.");
            }

            scheme = PreferredScheme ?? scheme;
            authority = NormalizeAuthority(authority);
            path = NormalizePath(path);
            var uri = base.CreateUrl(
                scheme,
                authority,
                path);
            return uri;
        }

        protected virtual string NormalizeAuthority(string authority)
        {
            if (IsUserInfoIgnored)
            {
                // Remove the user info, if required
                var atIndex = authority.IndexOf('@');
                if (atIndex != -1)
                {
                    authority = authority[(atIndex + 1)..];
                }
            }

            return authority;
        }

        protected virtual string NormalizeFragment(string fragment)
        {
            if (IsFragmentIgnored)
            {
                fragment = string.Empty;
            }
            else
            {
                fragment = fragment.Equals("#")
                    ? string.Empty
                    : $"#{fragment[1..]}";
            }

            return fragment;
        }

        protected override string NormalizePath(string path)
        {
            if (RemovableDirectoryIndexNames.Count > 0)
            {
                // Determine the filename, if any
                var slashIndex = path.LastIndexOf('/');
                var filename = path[(slashIndex + 1)..];
                var periodIndex = filename.LastIndexOf('.');
                if (periodIndex != -1)
                {
                    var name = filename.Substring(0, periodIndex);
                    if (RemovableDirectoryIndexNames.Contains(name))
                    {
                        // The existence of a slash indicates that a directory exists
                        path = $"{path.Substring(0, slashIndex)}/";
                    }
                }
            }

            return path;
        }

        protected virtual string NormalizeQuery(string query)
        {
            query = query[1..];
            if (query == string.Empty)
            {
                return string.Empty;
            }

            var keyToValues = new SortedDictionary<string, SortedSet<string>>();
            var tokens = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                string key;
                string value;
                var index = token.IndexOf('=');
                if (index == -1)
                {
                    // e.g. a
                    key = token;
                    value = string.Empty;
                }
                else
                {
                    key = token.Substring(0, index + 1);
                    if (key.Equals("="))
                    {
                        // Browsers do not send a value if a key is empty

                        // e.g. =

                        // e.g. =b
                        continue;
                    }

                    // e.g. a=
                    //
                    // e.g. a = b
                    //
                    // e.g. a=
                    //
                    // e.g. =
                    value = token[(index + 1)..];
                }

                SortedSet<string> values;
                var hasKey = keyToValues.ContainsKey(key);
                if (hasKey)
                {
                    values = keyToValues[key];
                }
                else
                {
                    values = new();
                    keyToValues.Add(key, values);
                }

                values.Add(value);
            }

            if (keyToValues.Count == 0)
            {
                query = string.Empty;
            }
            else
            {
                var builder = new StringBuilder();
                foreach (KeyValuePair<string, SortedSet<string>> keyToValue in keyToValues)
                {
                    var key = keyToValue.Key;
                    foreach (string value in keyToValue.Value)
                    {
                        builder.Append($"&{key}{value}");
                    }
                }

                // Remove leading '&'
                builder.Remove(0, 1);

                builder.Insert(0, '?');
                query = builder.ToString();
                query = NormalizePercentEncoding(query);
            }

            return query;
        }
    }
}