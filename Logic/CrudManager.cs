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

namespace Framework.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Abstraction;

    using AutoMapper;

    using Framework.Localization;
    using Framework.Tools;

    using Repository.Abstraction;

    public abstract class CrudManager<T, TKey, TEntity> : GetManager<T, TKey, TEntity>, ICrudManager<T, TKey> where T : class where TEntity : class
    {
        #region private /ctr

        private readonly IMapper                        _mapper;
        private readonly ICrudRepository<TEntity, TKey> _repository;
        private readonly IUnitOfWork                    _unitOfWork;

        protected CrudManager(IUnitOfWork unitOfWork, ICrudRepository<TEntity, TKey> repository, IMapper mapper) : base(unitOfWork, repository, mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _mapper     = mapper;
        }

        #endregion

        #region Add

        public async Task<TKey> Add(T value)
        {
            return (await Add(new List<T>() { value })).First();
        }

        public async Task<IEnumerable<TKey>> Add(IEnumerable<T> values)
        {
            var myValues = values.ToICollection();
            using (var trans = _unitOfWork.BeginTransaction())
            {
                await ValidateDto(myValues, ValidationType.AddValidation);
                var entities = MapFromDtos(myValues, ValidationType.AddValidation).ToICollection();

                foreach (var entity in entities)
                {
                    AddEntity(entity);
                }

                _repository.AddRange(entities);

                await CommitTransaction(trans);

                return entities.Select(GetKey);
            }
        }

        #endregion

        #region Delete

        public async Task Delete(T value)
        {
            await Delete(new[] { value });
        }

        public async Task Delete(IEnumerable<T> values)
        {
            var myValues = values.ToICollection();

            using (var trans = _unitOfWork.BeginTransaction())
            {
                await ValidateDto(myValues, ValidationType.DeleteValidation);
                var entities = MapFromDtos(myValues, ValidationType.DeleteValidation).ToICollection();

                foreach (var entity in entities)
                {
                    DeleteEntity(entity);
                }

                _repository.DeleteRange(entities);

                await CommitTransaction(trans);
            }
        }

        public async Task Delete(TKey key)
        {
            await Delete(new[] { key });
        }

        public async Task Delete(IEnumerable<TKey> keys)
        {
            using (var trans = _unitOfWork.BeginTransaction())
            {
                var entities = await _repository.GetTracking(keys);

                foreach (var entity in entities)
                {
                    DeleteEntity(entity);
                }

                _repository.DeleteRange(entities);

                await CommitTransaction(trans);
            }
        }

        #endregion

        #region Update

        public async Task Update(T value)
        {
            await Update(new[] { value });
        }

        public async Task Update(IEnumerable<T> values)
        {
            var myValues = values.ToICollection();

            using (var trans = _unitOfWork.BeginTransaction())
            {
                await ValidateDto(myValues, ValidationType.UpdateValidation);

                var entities     = MapFromDtos(myValues, ValidationType.UpdateValidation).ToICollection();
                var entitiesInDb = await _repository.GetTracking(entities.Select(GetKey));

                await Update(myValues, entitiesInDb, entities);
                await CommitTransaction(trans);
            }
        }

        protected async Task Update(IEnumerable<T> values, IList<TEntity> entitiesInDb, IEnumerable<TEntity> entities)
        {
            var myEntities = entities.ToICollection();
            var myValues   = values.ToICollection();

            var mergeJoin = entitiesInDb.Join(myEntities, GetKey, GetKey, (entityInDb, entity) => new { EntityInDb = entityInDb, Entity = entity }).ToList();

            if (myEntities.Count() != entitiesInDb.Count || myEntities.Count() != mergeJoin.Count())
            {
                throw new ArgumentException(ErrorMessages.ResourceManager.ToLocalizable(nameof(ErrorMessages.Framework_Logic_JoinResultDifferent)).Message());
            }

            foreach (var merged in mergeJoin)
            {
                UpdateEntity(myValues, merged.EntityInDb, merged.Entity);
            }

            await Task.CompletedTask;
        }

        #endregion

        #region internal

        protected async Task CommitTransaction(ITransaction trans)
        {
            try
            {
                await Modifying();
                await trans.CommitTransactionAsync();
                await Modified();
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                // Console.WriteLine(e);
                throw;
            }
        }

        #endregion

        #region Validadation and Modification overrides

        protected enum ValidationType
        {
            AddValidation,
            UpdateValidation,
            DeleteValidation
        }

        protected virtual async Task ValidateDto(IEnumerable<T> values, ValidationType validation)
        {
            foreach (var dto in values)
            {
                await ValidateDto(dto, validation);
            }
        }

#pragma warning disable 1998
        protected virtual async Task ValidateDto(T dto, ValidationType validation)
        {
        }

        protected virtual void AddEntity(TEntity entity)
        {
        }

        protected virtual void DeleteEntity(TEntity entityInDb)
        {
        }

        protected virtual void UpdateEntity(IEnumerable<T> dtos, TEntity entityInDb, TEntity entity)
        {
            UpdateEntity(entityInDb, entity);
        }

        protected virtual void UpdateEntity(TEntity entityInDb, TEntity values)
        {
            _repository.SetValueGraph(entityInDb, values);
        }

        protected virtual async Task Modifying()
        {
        }

        protected virtual async Task Modified()
        {
        }
#pragma warning restore 1998

        protected virtual IEnumerable<TEntity> MapFromDtos(IEnumerable<T> values, ValidationType validation)
        {
            return _mapper.Map<IEnumerable<T>, IEnumerable<TEntity>>(values);
        }

        #endregion
    }
}