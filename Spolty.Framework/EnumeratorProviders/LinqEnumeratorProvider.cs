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
    internal class LinqEnumeratorProvider : IEnumeratorProvider
    {
        private const string DynamicString = "Dynamic";
        private readonly MethodInfo _method;

        public LinqEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression)
        {
            Checker.CheckArgumentNull(entityType, "entityType");
            Checker.CheckArgumentNull(provider, "provider");
            Checker.CheckArgumentNull(expression, "expression");

            EntityType = entityType;
            QueryProvider = provider;
            Expression = expression;

            _method = GetType().GetMethod("LinkEntitySetsGeneric",
                                          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #region IEnumeratorProvider Members

        public Type EntityType { get; private set; }
        public Expression Expression { get; private set; }
        public IQueryProvider QueryProvider { get; private set; }

        public IEnumerator GetEnumerator()
        {
            IQueryable query = QueryProvider.CreateQuery(Expression);

            Type dynamicType = ReflectionHelper.GetGenericType(Expression.Type);
            if (dynamicType.Name.StartsWith(DynamicString))
            {
                IEnumerable resultValues = null;
                LinkEntitySets(Expression.Type, query, out resultValues);

                return resultValues.GetEnumerator();
            }

            return query.GetEnumerator();
        }

        #endregion

        private void LinkEntitySets(Type type, IEnumerable values, out IEnumerable resultValues)
        {
            MethodInfo genericMethod = _method.MakeGenericMethod(EntityType);
            var methodInvokers = new List<MethodInvoker>();
            resultValues =
                (IEnumerable) genericMethod.Invoke(this, new object[] {type, values, methodInvokers});
            foreach (MethodInvoker methodInvoker in methodInvokers)
            {
                methodInvoker.Invoke();
            }
        }

        private IEnumerable<T> LinkEntitySetsGeneric<T>(Type dynamicType, IEnumerable values,
                                                        ICollection<MethodInvoker> methodInvokers)
        {
            var result = new List<T>();
            Type genericType = ReflectionHelper.GetGenericType(dynamicType);
            List<PropertyInfo> enumerableProperties =
                genericType.GetProperties().Where(
                    pi => pi.PropertyType.GetInterface(typeof (IEnumerable).Name) != null).ToList();

            MemberInfo repositoryMember = ReflectionHelper.GetMemberInfo(genericType, MethodName.MainProperty);
            foreach (object value in values)
            {
                object repositoryValue = ReflectionHelper.GetValue(repositoryMember, value);

                result.Add((T) repositoryValue);

                foreach (PropertyInfo enumerableProperty in enumerableProperties)
                {
                    var propertyValues = (IEnumerable) ReflectionHelper.GetValue(enumerableProperty, value);
                    Type repositoryMemberType = ReflectionHelper.GetMemberType(repositoryMember);
                    MemberInfo entitySetMember = ReflectionHelper.GetMemberInfo(repositoryMemberType,
                                                                                enumerableProperty.Name);
                    object entitySetValues = ReflectionHelper.GetValue(entitySetMember, repositoryValue);
                    MethodInfo methodInfo =
                        ReflectionHelper.GetMemberType(entitySetMember).GetMethod(MethodName.SetSource);

                    var methodInvoker = new MethodInvoker
                                            {
                                                Method = methodInfo,
                                                Object = entitySetValues
                                            };
                    if (ReflectionHelper.GetGenericType(enumerableProperty.PropertyType).Name
                        .StartsWith(DynamicString))
                    {
                        Type entitySetGenericType =
                            ReflectionHelper.GetGenericType(ReflectionHelper.GetMemberType(entitySetMember));
                        MethodInfo genericMethod = _method.MakeGenericMethod(entitySetGenericType);
                        methodInvoker.Parameters = new[]
                                                       {
                                                           genericMethod.Invoke(this, new object[]
                                                                                          {
                                                                                              enumerableProperty.PropertyType,
                                                                                              propertyValues,
                                                                                              methodInvokers
                                                                                          })
                                                       };
                    }
                    else
                    {
                        methodInvoker.Parameters = new[] {propertyValues};
                    }
                    methodInvokers.Add(methodInvoker);
                }
            }

            return result;
        }

        #region Nested type: MethodInvoker

        private class MethodInvoker
        {
            public MethodInfo Method { get; set; }
            public object Object { get; set; }
            public object[] Parameters { get; set; }

            public object Invoke()
            {
                return Method.Invoke(Object, Parameters);
            }
        }

        #endregion
    }
}