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

// see: https://github.com/scottksmith95/LINQKit

using System;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Framework.Repository
{
    /// <summary> The Predicate Operator </summary>
    public enum PredicateOperator
    {
        Or,
        And
    }

    public static class PredicateBuilder
    {
        private class RebindParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public RebindParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _oldParameter)
                {
                    return _newParameter;
                }

                return base.VisitParameter(node);
            }
        }

        /// <summary> Start an expression </summary>
        public static ExpressionStarter<T> New<T>(Expression<Func<T, bool>> expr = null)
        {
            return new ExpressionStarter<T>(expr);
        }

        /// <summary> Create an expression with a stub expression true or false to use when the expression is not yet started. </summary>
        public static ExpressionStarter<T> New<T>(bool defaultExpression)
        {
            return new ExpressionStarter<T>(defaultExpression);
        }

        public static Expression<Func<T, bool>> Or<T>([NotNull] this Expression<Func<T, bool>> expr1, [NotNull] Expression<Func<T, bool>> expr2)
        {
            var expr2Body = new RebindParameterVisitor(expr2.Parameters[0], expr1.Parameters[0]).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, expr2Body), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>([NotNull] this Expression<Func<T, bool>> expr1, [NotNull] Expression<Func<T, bool>> expr2)
        {
            var expr2Body = new RebindParameterVisitor(expr2.Parameters[0], expr1.Parameters[0]).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2Body), expr1.Parameters);
        }

        /// <summary>
        /// Extends the specified source Predicate with another Predicate and the specified PredicateOperator.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="first">The source Predicate.</param>
        /// <param name="second">The second Predicate.</param>
        /// <param name="operator">The Operator (can be "And" or "Or").</param>
        /// <returns>Expression{Func{T, bool}}</returns>
        public static Expression<Func<T, bool>> Extend<T>(
            [NotNull] this Expression<Func<T, bool>> first,
            [NotNull]      Expression<Func<T, bool>> second,
            PredicateOperator                        @operator = PredicateOperator.Or)
        {
            return @operator == PredicateOperator.Or ? first.Or(second) : first.And(second);
        }

        /// <summary>
        /// Extends the specified source Predicate with another Predicate and the specified PredicateOperator.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="first">The source Predicate.</param>
        /// <param name="second">The second Predicate.</param>
        /// <param name="operator">The Operator (can be "And" or "Or").</param>
        /// <returns>Expression{Func{T, bool}}</returns>
        public static Expression<Func<T, bool>> Extend<T>([NotNull] this ExpressionStarter<T> first, [NotNull] Expression<Func<T, bool>> second, PredicateOperator @operator = PredicateOperator.Or)
        {
            return @operator == PredicateOperator.Or ? first.Or(second) : first.And(second);
        }
    }
}