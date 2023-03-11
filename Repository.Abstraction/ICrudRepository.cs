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

namespace Framework.Repository.Abstraction;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICrudRepository<TEntity, TKey> : IGetRepository<TEntity, TKey>
    where TEntity : class
{
    Task<IList<TEntity>> GetTrackingAllAsync(params string[] includeProperties);

    Task<TEntity> GetTrackingAsync(TKey key, params string[] includeProperties);

    Task<IList<TEntity>> GetTrackingAsync(IEnumerable<TKey> keys, params string[] includeProperties);

    Task AddAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> entities);

    void Delete(TEntity entity);

    void DeleteRange(IEnumerable<TEntity> entities);

    void SetValue(TEntity trackingEntity, TEntity values);

    void SetValueGraph(TEntity trackingEntity, TEntity values);

    void SetState(TEntity entity, MyEntityState state);

    Task SyncAsync(ICollection<TEntity> inDb, ICollection<TEntity> toDb, Func<TEntity, TEntity, bool> compareEntities, Action<TEntity> prepareForAdd);
}