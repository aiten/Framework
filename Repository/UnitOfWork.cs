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
    using System.Threading.Tasks;

    using Abstraction;

    using Framework.Localization;

    using Microsoft.EntityFrameworkCore;

    public class UnitOfWork<T> : IUnitOfWork
        where T : DbContext
    {
        public T Context { get; private set; }

        public UnitOfWork(T context)
        {
            Context = context;
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public async Task<int> ExecuteSqlCommand(string sql)
        {
            return await Context.Database.ExecuteSqlRawAsync(sql);
        }

        public async Task<int> ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return await Context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        #region Transaction

        public bool IsInTransaction()
        {
            return Context.Database.CurrentTransaction != null;
        }

        public ITransaction BeginTransaction()
        {
            if (IsInTransaction())
            {
                throw new ArgumentException(ErrorMessages.ResourceManager.ToLocalizable(nameof(ErrorMessages.Framework_Repository_NestedTransaction)).Message());
            }

            return new Transaction(this, Context.Database.BeginTransaction());
        }

        #endregion
    }
}