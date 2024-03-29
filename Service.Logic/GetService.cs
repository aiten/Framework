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

namespace Framework.Service.Logic;

using System.Collections.Generic;
using System.Threading.Tasks;

using Framework.Logic.Abstraction;

public abstract class GetService<T, TKey> : ServiceBase where T : class where TKey : notnull
{
    private readonly ICrudManager<T, TKey> _manager;

    protected GetService(ICrudManager<T, TKey> manager)
    {
        _manager = manager;
    }

    public async Task<T?> GetAsync(TKey id)
    {
        return await _manager.GetAsync(id);
    }

    public async Task<IEnumerable<T>> GetAsync(IEnumerable<TKey> ids)
    {
        return await _manager.GetAsync(ids);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _manager.GetAllAsync();
    }
}