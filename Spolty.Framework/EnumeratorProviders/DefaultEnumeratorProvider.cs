using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.ExpressionMakers.Linq;
using Spolty.Framework.Helpers;

namespace Spolty.Framework.EnumeratorProviders
{
    internal class DefaultEnumeratorProvider : IEnumeratorProvider
    {
        public DefaultEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression)
        {
            Checker.CheckArgumentNull(entityType, "entityType");
            Checker.CheckArgumentNull(provider, "provider");
            Checker.CheckArgumentNull(expression, "expression");

            EntityType = entityType;
            QueryProvider = provider;
            Expression = expression;
        }

        #region IEnumeratorProvider Members

        public Type EntityType { get; private set; }
        public Expression Expression { get; private set; }
        public IQueryProvider QueryProvider { get; private set; }

        public virtual IEnumerator GetEnumerator()
        {
            object returnValue = QueryProvider.CreateQuery(Expression);
            var values = (IEnumerable) returnValue;
            Type expressionType = ReflectionHelper.GetGenericType(Expression.Type);

            if (EntityType != expressionType)
            {
                var result = new List<object>();
            	MethodInfo methodInfo = expressionType.GetMethod("get_" + MethodName.MainProperty, ReflectionHelper.PublicMemberFlag);
            	Func<object, object> getMethod = ReflectionHelper.MakeDelegateMethod(expressionType, methodInfo);
            	foreach (object value in values)
                {
                    object repositoryValue = getMethod(value);
                    result.Add(repositoryValue);
                }

                return result.GetEnumerator();
            }
            return values.GetEnumerator();
        }

        #endregion
    }
}