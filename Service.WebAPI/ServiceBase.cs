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

namespace Framework.Service.WebAPI;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Framework.Pattern;
using Framework.Service.WebAPI.Uri;

public abstract class ServiceBase : DisposeWrapper
{
    public string BaseApi { get; set; } = string.Empty;

    protected abstract IScope<HttpClient> CreateScope();

    #region Read / GetAsync

    public async Task<IList<T>> ReadList<T>(string uri)
    {
        using (var scope = CreateScope())
        {
            var response = await scope.Instance.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<IList<T>>();
        }
    }

    public async Task<IList<T>> ReadList<T>(UriPathBuilder pathBuilder)
    {
        return await ReadList<T>(pathBuilder.Build());
    }

    public async Task<T> Read<T>(string uri)
    {
        using (var scope = CreateScope())
        {
            return await Read<T>(await scope.Instance.GetAsync(uri));
        }
    }

    public async Task<T> Read<T>(UriPathBuilder pathBuilder)
    {
        return await Read<T>(pathBuilder.Build());
    }

    public async Task<string> ReadString(UriPathBuilder pathBuilder)
    {
        return await ReadString(pathBuilder.Build());
    }

    public async Task<string> ReadString(string uri)
    {
        using (var scope = CreateScope())
        {
            var response = await scope.Instance.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    protected async Task<T> Read<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default(T)!;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<T>();
    }

    #endregion

    public UriPathBuilder CreatePathBuilder()
    {
        return new UriPathBuilder().AddPath(BaseApi);
    }
}