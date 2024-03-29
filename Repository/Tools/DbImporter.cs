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

namespace Framework.Repository.Tools;

using System;
using System.Collections.Generic;
using System.Linq;

using Framework.CsvImport;

using Microsoft.EntityFrameworkCore;

public class DbImporter
{
    private readonly DbContext _context;

    public string CsvDir { get; set; } = string.Empty;

    protected DbImporter(DbContext context)
    {
        _context = context;
    }

    #region Csv Import

    public IList<T> Read<T>(string fileName) where T : class
    {
        return ReadFromCsv<T>($@"{CsvDir}/{fileName}");
    }

    protected Dictionary<TKey, TEntity> ImportCsv<TKey, TEntity>(string fileName, Func<TEntity, TKey> getKey, Action<TEntity, TKey> setKey) where TEntity : class where TKey : notnull
    {
        var entities = Read<TEntity>(fileName);
        return PrepareAndAdd<TKey, TEntity>(entities, getKey, setKey);
    }

    private Dictionary<TKey, TEntity> PrepareAndAdd<TKey, TEntity>(IList<TEntity> items, Func<TEntity, TKey> getKey, Action<TEntity, TKey> setKey) where TEntity : class where TKey : notnull
    {
        var keyMap = items.ToDictionary(getKey, x => x);
        foreach (var entity in items)
        {
            setKey(entity, default!);
        }

        _context.Set<TEntity>().AddRange(items);
        return keyMap;
    }

    private static IList<T> ReadFromCsv<T>(string path) where T : class
    {
        var csvImport = new CsvImport<T>();
        var items     = csvImport.Read(path).ToList();
        return items;
    }

    #endregion

    #region fill from Db

    protected Dictionary<TKey, TEntity> ReadFromDb<TKey, TEntity>(Func<TEntity, TKey> getKey) where TEntity : class where TKey : notnull
    {
        var allEntities = _context.Set<TEntity>().ToList();
        var keyMap      = allEntities.ToDictionary(getKey, x => x);

        return keyMap;
    }

    #endregion
}