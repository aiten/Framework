﻿/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace Framework.Service.WebAPI.Uri;

using System.Linq;
using System.Web;

public class UriPathBuilder
{
    public string Path  { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;

    public string Build()
    {
        if (string.IsNullOrEmpty(Query))
        {
            return $"{Path}";
        }

        return $"{Path}?{Query}";
    }

    public UriPathBuilder AddPath<T>(T path)
    {
        return AddPath(path!.ToUriAsPath());
    }

    public UriPathBuilder AddPath(string path)
    {
        if (string.IsNullOrEmpty(Path))
        {
            Path = path;
        }
        else
        {
            if (Path[^1] != '/')
            {
                Path += '/';
            }

            Path += path;
        }

        return this;
    }

    public UriPathBuilder AddPath(string[] pathElements)
    {
        if (pathElements != null)
        {
            return AddPath(string.Join("/", pathElements.Select(HttpUtility.UrlEncode)));
        }

        return this;
    }

    public UriPathBuilder AddQuery(string query)
    {
        if (string.IsNullOrEmpty(Query))
        {
            Query = query;
        }
        else
        {
            Query += "&" + query;
        }

        return this;
    }

    public UriPathBuilder AddQuery(UriQueryBuilder filter)
    {
        return AddQuery(filter.ToString());
    }
}