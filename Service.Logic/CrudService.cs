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

namespace Framework.Service.Logic;

using System.Collections.Generic;
using System.Threading.Tasks;

using Framework.Logic.Abstraction;

public abstract class CrudService<T, TKey> : GetService<T, TKey> where T : class where TKey : notnull
{
    private readonly ICrudManager<T, TKey> _manager;

    protected CrudService(ICrudManager<T, TKey> manager) : base(manager)
    {
        _manager = manager;
    }

    public async Task<TKey> AddAsync(T value)
    {
        return await _manager.AddAsync(value);
    }

    public async Task<IEnumerable<TKey>> AddAsync(IEnumerable<T> values)
    {
        return await _manager.AddAsync(values);
    }

    public async Task DeleteAsync(T value)
    {
        await _manager.DeleteAsync(value);
    }

    public async Task DeleteAsync(IEnumerable<T> values)
    {
        await _manager.DeleteAsync(values);
    }

    public async Task DeleteAsync(TKey key)
    {
        await _manager.DeleteAsync(key);
    }

    public async Task DeleteAsync(IEnumerable<TKey> keys)
    {
        await _manager.DeleteAsync(keys);
    }

    public async Task UpdateAsync(T value)
    {
        await _manager.UpdateAsync(value);
    }

    public async Task UpdateAsync(IEnumerable<T> values)
    {
        await _manager.UpdateAsync(values);
    }
}