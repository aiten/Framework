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

namespace Framework.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;

    public abstract class RepositoryBase<TDbContext>
        where TDbContext : DbContext
    {
        #region Crt + Properties

        protected RepositoryBase(TDbContext dbContext)
        {
            Context = dbContext;
        }

        /// <summary>
        /// Gets the DbContext. Should be used rarely, instead use <see cref="Query{T}"/> and <see cref="TrackingQuery{T}"/>.
        /// </summary>
        protected TDbContext Context { get; private set; }

        #endregion

        public void Sync<TEntity>(ICollection<TEntity> inDb, ICollection<TEntity> toDb, Func<TEntity, TEntity, bool> compareEntities, Action<TEntity> prepareForAdd)
            where TEntity : class
        {
            //// 1. DeleteEntity from DB (in DB) and update
            var delete = new List<TEntity>();

            foreach (var entityInDb in inDb)
            {
                var entityToDb = toDb.FirstOrDefault(x => compareEntities(x, entityInDb));
                if (entityToDb != null && compareEntities(entityToDb, entityInDb))
                {
                    SetValue(entityInDb, entityToDb);
                }
                else
                {
                    delete.Add(entityInDb);
                }
            }

            foreach (var del in delete)
            {
                DeleteEntity(del);
            }

            //// 2. AddEntity To DB
            var toAdd = new List<TEntity>();

            foreach (var entityToDb in toDb)
            {
                var entityInDb = inDb.FirstOrDefault(x => compareEntities(x, entityToDb));
                if (entityInDb == null || compareEntities(entityToDb, entityInDb) == false)
                {
                    toAdd.Add(entityToDb);
                }
            }

            foreach (var add in toAdd)
            {
                prepareForAdd(add);
                AddEntity(add);
            }
        }

        #region Query

        protected IQueryable<T> QueryAsDbSet<T>()
            where T : class
        {
            return Context.Set<T>();
        }

        /// <summary>
        /// Returns an IQueryable of the Entity or query.
        /// Override to use Context.Query instead of DbContext.Set
        /// </summary>
        /// <typeparam name="T">Entity for which to return the IQueryable.</typeparam>
        /// <returns>Queryable with AsNoTracking() set.</returns>
        protected virtual IQueryable<T> GetQuery<T>()
            where T : class
        {
            return QueryAsDbSet<T>();
        }

        /// <summary>
        /// Returns an IQueryable of the Entity with AsNoTracking set. This should be the default.
        /// </summary>
        /// <typeparam name="T">Entity for which to return the IQueryable.</typeparam>
        /// <returns>Queryable with AsNoTracking() set.</returns>
        protected IQueryable<T> Query<T>()
            where T : class
        {
            return GetQuery<T>().AsNoTracking();
        }

        /// <summary>
        /// Gets an IQueryable that has tracking enabled.
        /// </summary>
        /// <typeparam name="T">Entity for which to return the IQueryable.</typeparam>
        /// <returns>Queryable with tracking enabled.</returns>
        protected IQueryable<T> TrackingQuery<T>()
            where T : class
        {
            return GetQuery<T>();
        }

        #endregion

        #region Entity

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> TrackingEntity<TEntity>(TEntity e)
            where TEntity : class
        {
            return Context.Entry(e);
        }

        protected void SetEntityPropertyOriginalValue<TEntity, TProperty>(TEntity entity, System.Linq.Expressions.Expression<System.Func<TEntity, TProperty>> columnDescriptor, TProperty value)
            where TEntity : class
        {
            TrackingEntity(entity).Property(columnDescriptor).OriginalValue = value;
        }

        protected TProperty GetEntityPropertyOriginalValue<TEntity, TProperty>(TEntity entity, System.Linq.Expressions.Expression<System.Func<TEntity, TProperty>> columnDescriptor)
            where TEntity : class
        {
            return TrackingEntity(entity).Property(columnDescriptor).OriginalValue;
        }

        protected void SetEntityPropertyCurrentValue<TEntity, TProperty>(TEntity entity, System.Linq.Expressions.Expression<System.Func<TEntity, TProperty>> columnDescriptor, TProperty value)
            where TEntity : class
        {
            TrackingEntity(entity).Property(columnDescriptor).CurrentValue = value;
        }

        protected TProperty GetEntityPropertyCurrentValue<TEntity, TProperty>(TEntity entity, System.Linq.Expressions.Expression<System.Func<TEntity, TProperty>> columnDescriptor)
            where TEntity : class
        {
            return TrackingEntity(entity).Property(columnDescriptor).CurrentValue;
        }

        protected virtual void SetEntityState<TEntity>(TEntity entity, EntityState state)
            where TEntity : class
        {
            TrackingEntity(entity).State = state;
        }

        protected virtual void SetValue<TEntity>(TEntity entity, object values)
            where TEntity : class
        {
            TrackingEntity(entity).CurrentValues.SetValues(values);
        }

        protected virtual void SetModified<TEntity>(TEntity entity)
            where TEntity : class
        {
            SetEntityState(entity, EntityState.Modified);
        }

        protected virtual void AddEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            Context.Add(entity);
        }

        protected virtual void AddEntities<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            Context.AddRange(entities);
        }

        protected virtual void DeleteEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            SetEntityState(entity, EntityState.Deleted);
        }

        protected virtual void DeleteEntities<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                DeleteEntity(entity);
            }
        }

        #endregion
    }
}