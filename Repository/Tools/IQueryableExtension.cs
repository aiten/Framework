/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// see: https://stackoverflow.com/questions/37527783/get-sql-code-from-an-entity-framework-core-iqueryablet

namespace Framework.Repository.Tools
{
    public static class IQueryableExtensions
    {
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            using (var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator())
            {
                var relationalCommandCache = enumerator.Private("_relationalCommandCache");
                var selectExpression       = relationalCommandCache.Private<SelectExpression>("_selectExpression");
                var factory                = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

                var sqlGenerator = factory.Create();
                var command      = sqlGenerator.GetCommand(selectExpression);

                string sql = command.CommandText;
                return sql;
            }
        }

        private static object Private(this    object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        private static T      Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }
}