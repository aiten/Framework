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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

public abstract class GetRepository<TDbContext, TEntity, TKey> : QueryRepository<TDbContext, TEntity>
    where TDbContext : DbContext where TEntity : class
{
    protected GetRepository(TDbContext dbContext)
        : base(dbContext)
    {
    }

    #region QueryProperties

    protected IQueryable<TEntity> QueryWithInclude => AddInclude(Query);

    protected IQueryable<TEntity> QueryWithOptional => AddOptionalWhere(Query);

    protected virtual FilterBuilder<TEntity, TKey> FilterBuilder { get; } = null;

    #endregion

    #region GetAsync

    protected async Task<IList<TEntity>> GetAllAsync(IQueryable<TEntity> query)
    {
        return await query.ToListAsync();
    }

    protected async Task<TEntity> GetAsync(IQueryable<TEntity> query, TKey key)
    {
        return await AddPrimaryWhere(query, key).FirstOrDefaultAsync();
    }

    protected async Task<IList<TEntity>> GetAsync(IQueryable<TEntity> query, IEnumerable<TKey> keys)
    {
        return await AddPrimaryWhereIn(query, keys).ToListAsync();
    }

    public async Task<IList<TEntity>> GetAllAsync()
    {
        return await GetAllAsync(AddInclude(QueryWithOptional));
    }

    public async Task<TEntity> GetAsync(TKey key)
    {
        return await GetAsync(QueryWithInclude, key);
    }

    public async Task<IList<TEntity>> GetAsync(IEnumerable<TKey> keys)
    {
        return await GetAsync(QueryWithInclude, keys);
    }

    #endregion

    #region overrides

    protected virtual IQueryable<TEntity> AddOptionalWhere(IQueryable<TEntity> query) => query;

    protected virtual IQueryable<TEntity> AddInclude(IQueryable<TEntity> query) => query;

    protected virtual IQueryable<TEntity> AddPrimaryWhere(IQueryable<TEntity> query, TKey key)
    {
        return FilterBuilder.PrimaryWhere(query, key);
    }

    protected virtual IQueryable<TEntity> AddPrimaryWhereIn(IQueryable<TEntity> query, IEnumerable<TKey> keys)
    {
        return FilterBuilder.PrimaryWhereIn(query, keys);
    }

    #endregion
}