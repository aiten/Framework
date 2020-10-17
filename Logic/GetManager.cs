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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Abstraction;

    using AutoMapper;

    using Repository.Abstraction;

    public abstract class GetManager<T, TKey, TEntity> : ManagerBase, IGetManager<T, TKey> where T : class where TEntity : class
    {
        private readonly IMapper                       _mapper;
        private readonly IGetRepository<TEntity, TKey> _repository;

        protected GetManager(IUnitOfWork unitOfWork, IGetRepository<TEntity, TKey> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        protected abstract TKey GetKey(TEntity entity);

        public async Task<IEnumerable<T>> GetAll()
        {
            return await MapToDto(await GetAllEntities());
        }

        public async Task<T> Get(TKey key)
        {
            return await MapToDto(await _repository.Get(key));
        }

        public async Task<IEnumerable<T>> Get(IEnumerable<TKey> keys)
        {
            return await MapToDto(await _repository.Get(keys));
        }

        protected virtual async Task<IList<TEntity>> GetAllEntities()
        {
            return await _repository.GetAll();
        }

        protected virtual async Task<T> SetDto(T dto)
        {
            return await Task.FromResult(dto);
        }

        protected virtual async Task<IEnumerable<T>> SetDto(IEnumerable<T> dtos)
        {
            foreach (var dto in dtos)
            {
                await SetDto(dto);
            }

            return dtos;
        }

        protected async Task<IEnumerable<T>> MapToDto(IList<TEntity> entities)
        {
            var dtos = _mapper.Map<IEnumerable<TEntity>, IEnumerable<T>>(entities);
            return await SetDto(dtos);
        }

        protected async Task<T> MapToDto(TEntity entity)
        {
            var dto = _mapper.Map<TEntity, T>(entity);
            return await SetDto(dto);
        }
    }
}