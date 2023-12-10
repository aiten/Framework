/*
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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Framework.Pattern;

public abstract class CrudServiceBase<T, TKey> : ServiceBase where T : class where TKey : IComparable
{
    protected abstract TKey GetKey(T value);

    private readonly HttpClient _httpClient;

    protected CrudServiceBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected override IScope<HttpClient> CreateScope()
    {
        return new ScopeInstance<HttpClient>(_httpClient);
    }

    public async Task<T?> GetAsync(TKey id)
    {
        return await Read<T>(CreatePathBuilder().AddPath(id));
    }

    public Task<IEnumerable<T>> GetAsync(IEnumerable<TKey> keys)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Read<IEnumerable<T>>(CreatePathBuilder());
    }

    public async Task<TKey> AddAsync(T value)
    {
        using (var scope = CreateScope())
        {
            var response = await scope.Instance.PostAsJsonAsync(CreatePathBuilder().Build(), value);

            response.EnsureSuccessStatusCode();
            return GetKey(await response.Content.ReadAsAsync<T>());
        }
    }

    public Task<IEnumerable<TKey>> AddAsync(IEnumerable<T> values)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(T value)
    {
        await DeleteAsync(GetKey(value));
    }

    public Task DeleteAsync(IEnumerable<T> values)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(TKey key)
    {
        using (var scope = CreateScope())
        {
            var response = await scope.Instance.DeleteAsync(CreatePathBuilder().AddPath(key).Build());
            response.EnsureSuccessStatusCode();
        }
    }

    public Task DeleteAsync(IEnumerable<TKey> keys)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(T value)
    {
        using (var scope = CreateScope())
        {
            var response = await scope.Instance.PutAsJsonAsync(CreatePathBuilder().AddPath(GetKey(value)).Build(), value);
            response.EnsureSuccessStatusCode();
        }
    }

    public Task UpdateAsync(IEnumerable<T> values)
    {
        throw new NotImplementedException();
    }
}