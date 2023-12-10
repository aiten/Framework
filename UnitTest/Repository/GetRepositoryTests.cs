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

namespace Framework.UnitTest.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Framework.Repository.Abstraction;

using Microsoft.EntityFrameworkCore;

public class GetRepositoryTests<TDbContext, TEntity, TKey, TIRepository> : UnitTestBase
    where TEntity : class where TIRepository : IGetRepository<TEntity, TKey> where TKey : notnull where TDbContext : DbContext
{
    public required Func<GetTestDbContext<TDbContext, TEntity, TKey, TIRepository>> CreateTestDbContext;

    public required Func<TEntity, TKey>   GetEntityKey;
    public required Action<TEntity, TKey> SetEntityKey;

    public Func<TEntity, object>   GetEntityState = entity => null!;
    public Action<TEntity, object> SetEntityState = (entity, o) => { };

    public required Func<TEntity, TEntity, bool> CompareEntity;

    public async Task<IList<TEntity>> GetAll()
    {
        using (var ctx = CreateTestDbContext())
        {
            var entities = await ctx.Repository.GetAllAsync();
            entities.Should().NotBeNull();
            return entities;
        }
    }

    public async Task<TEntity> GetOK(TKey key)
    {
        using (var ctx = CreateTestDbContext())
        {
            var entity = await ctx.Repository.GetAsync(key);
            entity.Should().BeOfType(typeof(TEntity));
            entity.Should().NotBeNull();
            return entity!;
        }
    }

    public async Task<IList<TEntity>> GetOK(IEnumerable<TKey> keys)
    {
        using (var ctx = CreateTestDbContext())
        {
            var entities = await ctx.Repository.GetAsync(keys);
            entities.Should().NotBeNull();
            return entities;
        }
    }

    public async Task GetNotExist(TKey key)
    {
        using (var ctx = CreateTestDbContext())
        {
            var entity = await ctx.Repository.GetAsync(key);
            entity.Should().BeNull();
        }
    }
}