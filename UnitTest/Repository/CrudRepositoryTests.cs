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

namespace Framework.UnitTest.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Framework.Repository;
using Framework.Repository.Abstraction;

using Microsoft.EntityFrameworkCore;

public class CrudRepositoryTests<TDbContext, TEntity, TKey, TIRepository> : GetRepositoryTests<TDbContext, TEntity, TKey, TIRepository>
    where TEntity : class where TIRepository : ICrudRepository<TEntity, TKey> where TKey : notnull where TDbContext : DbContext
{
    public async Task<TEntity> GetTrackingOK(TKey key)
    {
        using (var ctx = CreateTestDbContext())
        {
            var entity = await ctx.Repository.GetTrackingAsync(key);
            entity.Should().NotBeNull();
            entity.Should().BeOfType(typeof(TEntity));
            return entity!;
        }
    }

    public async Task AddUpdateDelete(Func<TEntity> createTestEntity, Action<TEntity> updateEntity)
    {
        var allWithoutAdd = await GetAll();
        allWithoutAdd.Should().NotBeNull();

        // first add entity

        TKey   key;
        object entityState;

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityToAdd = createTestEntity();
            await ctx.Repository.AddAsync(entityToAdd);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();

            key         = GetEntityKey(entityToAdd);
            entityState = GetEntityState(entityToAdd);
        }

        var allWithAdd = await GetAll();
        allWithAdd.Should().NotBeNull();
        allWithAdd.Should().HaveCount(allWithoutAdd.Count + 1);

        // read again and update 

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entity = await ctx.Repository.GetTrackingAsync(key);
            entity.Should().NotBe(null);
            GetEntityKey(entity!).Should().Be(key);
            CompareEntity(createTestEntity(), entity!).Should().BeTrue();
            updateEntity(entity!);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entity = await ctx.Repository.GetAsync(key);
            entity.Should().NotBeNull();
            GetEntityKey(entity!).Should().Be(key);
            entityState = GetEntityState(entity!);
        }

        // update (with method update)

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entity = createTestEntity();
            SetEntityKey(entity, key);
            SetEntityState(entity, entityState);

            await ctx.Repository.UpdateAsync(key, entity);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again and delete 

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entity = await ctx.Repository.GetTrackingAsync(key);
            entity.Should().NotBeNull();
            GetEntityKey(entity!).Should().Be(key);

            var compareEntity = createTestEntity();
            CompareEntity(compareEntity, entity!).Should().BeTrue();

            await ctx.Repository.DeleteAsync(entity!);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again to test is not exist

        using (var ctx = CreateTestDbContext())
        {
            var entity = await ctx.Repository.GetTrackingAsync(key);
            entity.Should().BeNull();
        }
    }

    public async Task AddUpdateDeleteBulk(Func<ICollection<TEntity>> createTestEntities, Action<IEnumerable<TEntity>> updateEntities)
    {
        var allWithoutAdd = await GetAll();
        allWithoutAdd.Should().NotBeNull();

        // first add entity

        IEnumerable<TKey> keys;
        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entitiesToAdd = createTestEntities();
            await ctx.Repository.AddRangeAsync(entitiesToAdd);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();

            keys = entitiesToAdd.Select(GetEntityKey).ToList();
        }

        var allWithAdd = await GetAll();
        allWithAdd.Should().NotBeNull();
        allWithAdd.Should().HaveCount(allWithoutAdd.Count + keys.Count());

        // read again and update 
        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entities        = await ctx.Repository.GetTrackingAsync(keys);
            var compareEntities = createTestEntities();
            for (var i = 0; i < compareEntities.Count; i++)
            {
                GetEntityKey(entities.ElementAt(i)).Should().Be(keys.ElementAt(i));
                CompareEntity(compareEntities.ElementAt(i), entities.ElementAt(i)).Should().BeTrue();
            }

            updateEntities(entities);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again
        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entities = await ctx.Repository.GetAsync(keys);
            for (var i = 0; i < entities.Count; i++)
            {
                GetEntityKey(entities.ElementAt(i)).Should().Be(keys.ElementAt(i));
            }
        }

        // read again and delete 

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entities = await ctx.Repository.GetTrackingAsync(keys);

            var compareEntities = createTestEntities();
            updateEntities(compareEntities);

            for (var i = 0; i < compareEntities.Count; i++)
            {
                GetEntityKey(entities.ElementAt(i)).Should().Be(keys.ElementAt(i));
                CompareEntity(compareEntities.ElementAt(i), entities.ElementAt(i)).Should().BeTrue();
            }

            await ctx.Repository.DeleteRangeAsync(entities);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again to test if not exist

        using (var ctx = CreateTestDbContext())
        {
            var entities = await ctx.Repository.GetTrackingAsync(keys);
            entities.Should().HaveCount(0);
        }
    }

    public async Task AddRollBack(Func<TEntity> createTestEntity)
    {
        // first add entity

        TKey key;
        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityToAdd = createTestEntity();
            await ctx.Repository.AddAsync(entityToAdd);

            await ctx.UnitOfWork.SaveChangesAsync();

            // await trans.CommitTransactionAsync(); => no commit => Rollback

            key = GetEntityKey(entityToAdd);
        }

        // read again to test is not exist

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entity = await ctx.Repository.GetTrackingAsync(key);
            entity.Should().BeNull();
        }
    }

    public async Task Store(Func<TEntity> createTestEntity, Action<TEntity> updateEntity)
    {
        // test if entry not exist in DB

        var key = GetEntityKey(createTestEntity());

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityToTest = createTestEntity();
            var notFound     = await ctx.Repository.GetAsync(key);
            notFound.Should().BeNull();
        }

        // first add entity
        // only useful if key is no identity

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityToAdd = createTestEntity();
            await ctx.Repository.StoreAsync(entityToAdd, key);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // Read and UpdateAsync Entity
        // only useful if key is no identity

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityInDb = await ctx.Repository.GetAsync(key);
            entityInDb.Should().NotBeNull();
            CompareEntity(createTestEntity(), entityInDb!).Should().BeTrue();
        }

        // modify existing

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityToUpdate = createTestEntity();
            updateEntity(entityToUpdate);

            await ctx.Repository.StoreAsync(entityToUpdate, key);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }

        // read again (modified)

        using (var ctx = CreateTestDbContext())
        using (var trans = ctx.UnitOfWork.BeginTransaction())
        {
            var entityInDb = await ctx.Repository.GetAsync(key);
            entityInDb.Should().NotBeNull();

            var entityToCompare = createTestEntity();
            updateEntity(entityToCompare);

            CompareEntity(entityToCompare, entityInDb!).Should().BeTrue();

            await ctx.Repository.DeleteAsync(entityInDb!);

            await ctx.UnitOfWork.SaveChangesAsync();
            await trans.CommitTransactionAsync();
        }
    }
}