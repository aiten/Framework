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

namespace Framework.Repository;

using System;
using System.Threading.Tasks;

using Abstraction;

using Framework.Localization;

using Microsoft.EntityFrameworkCore.Storage;

public class Transaction : ITransaction
{
    public IUnitOfWork UnitOfWork { get; private set; }

    public Transaction(IUnitOfWork unitOfWork, IDbContextTransaction dbTran)
    {
        UnitOfWork = unitOfWork;
        _dbTran    = dbTran;
    }

    #region Transaction

    private IDbContextTransaction? _dbTran;

    public bool InTransaction => _dbTran != null;

    private void CheckInTransaction()
    {
        if (InTransaction == false)
        {
            throw new ArgumentException(ErrorMessages.ResourceManager.ToLocalizable(nameof(ErrorMessages.Framework_Repository_TransactionNotStarted)).Message());
        }
    }

    public async Task SaveChangesAsync()
    {
        CheckInTransaction();
        await UnitOfWork.SaveChangesAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await SaveChangesAsync();
        await _dbTran?.CommitAsync()!;
        _dbTran = null;
    }

    public void RollbackTransaction()
    {
        CheckInTransaction();

        _dbTran?.Rollback();
        _dbTran = null;
    }

    #endregion

    #region dispose

    public void Dispose()
    {
        if (InTransaction)
        {
            // if commit is not called, rollback transaction now
            RollbackTransaction();
        }
    }

    #endregion
}