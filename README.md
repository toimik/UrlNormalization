![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/nurhafiz/e5f4b0c6225c1f67a4ad6d231dcf3ec5/raw/UrlNormalization-coverage.json)
![Nuget](https://img.shields.io/nuget/v/Toimik.UrlNormalization)

# Toimik.UrlNormalization

.NET 6 C# [URL normalizer](https://en.wikipedia.org/wiki/URI_normalization).

## Features

URL normalization, also known as URL canonicalization, is the process of normalizing (standardizing) the text representation
of a URL to determine if differently-formatted URLs are identical.

#### All URLs

- Duplicate slashes are removed  
  `file://example.com/foo//bar.html` &#8594; `file://example.com/foo/bar.html`

- Default port is removed  
  `ftp://example.com:21/` &#8594; `ftp://example.com/`

- Dot-segments are removed  
  `file://example.com/foo/./bar/baz/../qux` &#8594; `file://example.com/foo/bar/qux`

- Empty path is converted to "/"  
  `ftp://example.com` &#8594; `ftp://example.com/`

- Percent-encoded triplets are uppercased  
  `ftp://example.com/foo%2a` &#8594; `ftp://example.com/foo%2A`

- Percent-encoded triplets of unreserved characters are decoded  
  `ftp://example.com/%7Efoo` &#8594; `ftp://example.com/~foo`

- Scheme and host are lowercased  
  `FTP://User@Example.COM/Foo` &#8594; `ftp://User@example.com/Foo`
  
#### HTTP-specific URLs

- Directory index can be removed (optional, via `removableDirectoryIndexNames`)  
  `http://example.com/default.asp` &#8594; `http://example.com/`  
  `http://example.com/a/index.html` &#8594; `http://example.com/a/`

- Fragment can be removed (optional, via `isFragmentIgnored`)  
  `http://example.com/bar.html#section1` &#8594; `http://example.com/bar.html`

- Scheme can be changed (optional, via `PreferredScheme`)  
  `https://example.com/` &#8594; `http://example.com/`

- Query parameters are sorted  
  `http://example.com/display?lang=en&article=fred` &#8594; `http://example.com/display?article=fred&lang=en`

- User-info can be removed (optional, via `isUserInfoIgnored`)  
  `http://user:password@example.com` &#8594; `http://example.com/`

- Empty query is removed  
  `http://example.com/display?` &#8594; `http://example.com/display`


## Quick Start

### Installation

#### Package Manager

```command
PM> Install-Package Toimik.UrlNormalization
```

#### .NET CLI

```command
> dotnet add package Toimik.UrlNormalization
```

### Usage

#### UrlNormalizer.cs

```c# 
// Use default arguments
// var normalizer = new UrlNormalizer();

// Use custom arguments
var normalizer = new UrlNormalizer(isAdjacentSlashesCollapsed: false);

var url = ...
var normalizedlUrl = normalizer.Normalize(url);
```

#### HttpUrlNormalizer.cs

```c# 
// Use default arguments
// var normalizer = new HttpUrlNormalizer();

// Use custom arguments
var normalizer = new HttpUrlNormalizer(
    preferredScheme: "https",
    isUserInfoIgnored: false,
    removableDirectoryIndexNames: new HashSet<string>(0), // override the default
    isFragmentIgnored: false);

var url = ...
var normalizedlUrl = normalizer.Normalize(url);
```