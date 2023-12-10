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

namespace Framework.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Framework.Repository.Abstraction;

using Microsoft.EntityFrameworkCore;

public abstract class CrudRepository<TDbContext, TEntity, TKey> : GetRepository<TDbContext, TEntity, TKey>
    where TDbContext : DbContext where TEntity : class where TKey : notnull
{
    protected CrudRepository(TDbContext dbContext)
        : base(dbContext)
    {
    }

    protected IQueryable<TEntity> TrackingQueryWithInclude(params string[] includeProperties) => AddInclude(TrackingQuery, includeProperties);

    protected IQueryable<TEntity> TrackingQueryWithOptional => AddOptionalWhere(TrackingQuery);

    #region GetAsync Tracking

    public async Task<IList<TEntity>> GetTrackingAllAsync(params string[] includeProperties)
    {
        return await GetAllAsync(AddInclude(TrackingQueryWithOptional));
    }

    public async Task<TEntity?> GetTrackingAsync(TKey key, params string[] includeProperties)
    {
        return await GetAsync(TrackingQueryWithInclude(includeProperties), key);
    }

    public async Task<IList<TEntity>> GetTrackingAsync(IEnumerable<TKey> keys, params string[] includeProperties)
    {
        return await GetAsync(TrackingQueryWithInclude(includeProperties), keys);
    }

    #endregion

    #region CRUD

    public async Task AddAsync(TEntity entity)
    {
        await AddEntityAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await AddEntitiesAsync(entities);
    }

    public async Task DeleteAsync(TEntity entity)
    {
        DeleteEntity(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        DeleteEntities(entities);
        await Task.CompletedTask;
    }

    public void SetState(TEntity entity, MyEntityState state)
    {
        SetEntityState(entity, (Microsoft.EntityFrameworkCore.EntityState)state);
    }

    public async Task SetValueAsync(TEntity entity, TEntity values)
    {
        AssignValues(entity, values);
        base.SetValue(entity, values);
        await Task.CompletedTask;
    }

    public async Task SetValueGraphAsync(TEntity trackingEntity, TEntity values)
    {
        await AssignValuesGraphAsync(trackingEntity, values);
    }

    protected virtual void AssignValues(TEntity trackingEntity, TEntity values)
    {
    }

    protected virtual async Task AssignValuesGraphAsync(TEntity trackingEntity, TEntity values)
    {
        SetValue(trackingEntity, values);
        await Task.CompletedTask;
    }

    #endregion

    public async Task SyncAsync(ICollection<TEntity> inDb,
        ICollection<TEntity>                         toDb,
        Func<TEntity, TEntity, bool>                 compareEntity,
        Action<TEntity>                              prepareForAdd)
    {
        await SyncAsync<TEntity>(inDb, toDb, compareEntity, prepareForAdd);
    }
}