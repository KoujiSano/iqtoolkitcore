﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections;
using System.Reflection;

namespace IQToolkit
{
    /// <summary>
    /// Optional interface for <see cref="IQueryProvider"/> to implement <see cref="Query{T}.QueryText"/> property.
    /// </summary>
    public interface IQueryText
    {
        string GetQueryText(Expression expression);
    }

    /// <summary>
    /// A default implementation of IQueryable for use with QueryProvider
    /// </summary>
    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        private readonly IQueryProvider provider;
        private readonly Expression expression;

        public Query(IQueryProvider provider)
            : this(provider, null)
        {
        }

        public Query(IQueryProvider provider, Type? staticType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }

            this.provider = provider;
            this.expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<T>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            this.provider = provider;
            this.expression = expression;
        }

        public Expression Expression
        {
            get { return this.expression; }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this.provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var result = provider.Execute(this.expression) as IEnumerable<T>;
            if (result == null)
            {
                throw new InvalidOperationException();
            }
            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = provider.Execute(this.expression) as IEnumerable;
            if (result == null)
            {
                throw new InvalidOperationException();
            }
            return result.GetEnumerator();
        }

        public override string ToString()
        {
            if (this.expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)this.expression).Value == this)
            {
                return "Query(" + typeof(T) + ")";
            }
            else
            {
                return this.expression.ToString();
            }
        }

        public string QueryText
        {
            get 
            {
                IQueryText? iqt = this.provider as IQueryText;
                if (iqt != null)
                {
                    return iqt.GetQueryText(this.expression);
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
