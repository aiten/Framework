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

namespace Framework.Pattern;

using System;

using Microsoft.Extensions.DependencyInjection;

// Factory/Scope using Resolve of dependencyInjection

public sealed class ScopeResolve<T> : IScope<T>, IDisposable where T : class
{
    private readonly IServiceScope _scope;
    private readonly T             _instance;

    private bool _isDisposed;

    public ScopeResolve(IServiceScope scope, T instance)
    {
        _scope    = scope;
        _instance = instance;
    }

    public T Instance
    {
        get
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("this", "Dispose must not be called twice.");
            }

            return _instance;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _scope.Dispose();
    }
}