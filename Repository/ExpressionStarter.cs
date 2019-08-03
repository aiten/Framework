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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
#if !(NET35 || WINDOWS_APP || NETSTANDARD || PORTABLE40 || UAP)
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
#endif

namespace Framework.Repository
{
    /// <summary>
    /// ExpressionStarter{T} which eliminates the default 1=0 or 1=1 stub expressions
    /// </summary>
    /// <typeparam name="T">The type</typeparam>

    public class ExpressionStarter<T>
    {
        internal ExpressionStarter() : this(false) { }

        internal ExpressionStarter(bool defaultExpression)
        {
            if (defaultExpression)
                DefaultExpression = f => true;
            else
                DefaultExpression = f => false;
        }

        internal ExpressionStarter(Expression<Func<T, bool>> exp) : this(false)
        {
            _predicate = exp;
        }

        /// <summary>The actual Predicate. It can only be set by calling Start.</summary>
        private Expression<Func<T, bool>> Predicate => (IsStarted || !UseDefaultExpression) ? _predicate : DefaultExpression;

        private Expression<Func<T, bool>> _predicate;

        /// <summary>Determines if the predicate is started.</summary>
        public bool IsStarted => _predicate != null;

        /// <summary> A default expression to use only when the expression is null </summary>
        public bool UseDefaultExpression => DefaultExpression != null;

        /// <summary>The default expression</summary>
        public Expression<Func<T, bool>> DefaultExpression { get; set; }

        /// <summary>Set the Expression predicate</summary>
        /// <param name="exp">The first expression</param>
        public Expression<Func<T, bool>> Start(Expression<Func<T, bool>> exp)
        {
            if (IsStarted)
                throw new Exception("Predicate cannot be started again.");

            return _predicate = exp;
        }

        /// <summary>Or</summary>
        public Expression<Func<T, bool>> Or([NotNull] Expression<Func<T, bool>> expr2)
        {
            return (IsStarted) ? _predicate = Predicate.Or(expr2) : Start(expr2);
        }

        /// <summary>And</summary>
        public Expression<Func<T, bool>> And([NotNull] Expression<Func<T, bool>> expr2)
        {
            return (IsStarted) ? _predicate = Predicate.And(expr2) : Start(expr2);
        }

        /// <summary> Show predicate string </summary>
        public override string ToString()
        {
            return Predicate == null ? null : Predicate.ToString();
        }

        #region Implicit Operators
        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator Expression<Func<T, bool>>(ExpressionStarter<T> right)
        {
            return right == null ? null : right.Predicate;
        }

        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator Func<T, bool>(ExpressionStarter<T> right)
        {
            return right == null ? null : (right.IsStarted || right.UseDefaultExpression) ? right.Predicate.Compile() : null;
        }

        /// <summary>
        /// Allows this object to be implicitely converted to an Expression{Func{T, bool}}.
        /// </summary>
        /// <param name="right"></param>
        public static implicit operator ExpressionStarter<T>(Expression<Func<T, bool>> right)
        {
            return right == null ? null : new ExpressionStarter<T>(right);
        }
        #endregion

        #region Implement Expression<TDelagate> methods and properties

        public Func<T, bool> Compile() { return Predicate.Compile(); }

        public Func<T, bool> Compile(DebugInfoGenerator debugInfoGenerator) { return Predicate.Compile(debugInfoGenerator); }

        public Expression<Func<T, bool>> Update(Expression body, IEnumerable<ParameterExpression> parameters) { return Predicate.Update(body, parameters); }

        #endregion

        #region Implement LamdaExpression methods and properties

        public Expression Body => Predicate.Body;

        public ExpressionType NodeType => Predicate.NodeType;

        public ReadOnlyCollection<ParameterExpression> Parameters => Predicate.Parameters;

        public Type Type => Predicate.Type;

        public string Name => Predicate.Name;

        public Type ReturnType => Predicate.ReturnType;

        public bool TailCall => Predicate.TailCall;

        #endregion

        #region Implement Expression methods and properties

        public virtual bool CanReduce => Predicate.CanReduce;
        
        #endregion
    }
}