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

namespace Framework.Tools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class ListExtensions
{
    #region Async

    public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
    {
        return await Task.WhenAll(source.Select(async s => await method(s)));
    }

    public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<IEnumerable<TResult>>> method)
    {
        return (await Task.WhenAll(source.Select(async s => await method(s)))).SelectMany(s => s);
    }

    public static async Task<IList<TResult>> SelectManyAsync<TSource, TResult>(this IList<TSource> source, Func<TSource, Task<IList<TResult>>> method)
    {
        return (await Task.WhenAll(source.Select(async s => await method(s)))).SelectMany(s => s).ToList();
    }

    #endregion

    public static ICollection<T> ToICollection<T>(this IEnumerable<T> list)
    {
        var collection = list as ICollection<T>;
        return collection ?? list.ToList();
    }

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int size)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, @"Must not be < 1");
        }

        var listList = new List<IEnumerable<T>>();
        var count    = 0;
        var lastList = new List<T>();

        foreach (var element in list)
        {
            if ((count % size) == 0)
            {
                lastList = new List<T>();
                listList.Add(lastList);
            }

            lastList.Add(element);
            count++;
        }

        return listList;
    }

    public static IEnumerable<IEnumerable<T>> SplitBefore<T>(this IEnumerable<T> list, Func<T, bool> askSplitBefore)
    {
        var       listList = new List<IEnumerable<T>>();
        IList<T>? lastList = null;

        foreach (var element in list)
        {
            if (askSplitBefore(element) || lastList == null)
            {
                lastList = new List<T>();
                listList.Add(lastList);
            }

            lastList.Add(element);
        }

        return listList;
    }

    public static IEnumerable<IEnumerable<T>> SplitAfter<T>(this IEnumerable<T> list, Func<T, bool> askSplitAfter)
    {
        var       listList = new List<IEnumerable<T>>();
        IList<T>? lastList = null;

        foreach (var element in list)
        {
            if (lastList == null)
            {
                lastList = new List<T>();
                listList.Add(lastList);
            }

            lastList.Add(element);

            if (askSplitAfter(element))
            {
                lastList = null;
            }
        }

        return listList;
    }

    public static IEnumerable<T> Select<T>(this IEnumerable list, PropertyInfo pi)
    {
        IList<T> returnList = new List<T>();

        foreach (var element in list)
        {
            var val = (T)pi.GetValue(element)!;
            returnList.Add(val);
        }

        return returnList;
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
    {
        return items.GroupBy(property).Select(x => x.First());
    }
}