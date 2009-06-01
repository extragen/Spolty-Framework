using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;
using Spolty.Framework.Storages;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class ExpressionMaker : IExpressionMaker, ISkipExpressionMaker, ITakeExpressionMaker, IUnionExpressionMaker,
                                     IExceptExpressionMaker, IDistinctExpressionMaker
    {
        private const string ParameterDictionaryKey = "ParameterDictionary";
        protected static Type EnumerableType;
        protected static Type IEnumerableType;
        protected static Type QueryableType;
        private readonly IExpressionMakerFactory _factory;

        #region Constructors

        static ExpressionMaker()
        {
            QueryableType = typeof (Queryable);
            EnumerableType = typeof (Enumerable);
            IEnumerableType = typeof (IEnumerable);
        }

        public ExpressionMaker(IExpressionMakerFactory factory)
        {
            _factory = factory;
        }

        #endregion

        #region IExpressionMaker Members

        public IExpressionMakerFactory Factory
        {
            get { return _factory; }
        }

        #endregion

        #region IExceptExpressionMaker Members

        Expression IExceptExpressionMaker.Make(Expression source, Expression except)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(except, "except");

            Type sourceType = GetTemplateType(source);
            Type exceptType = GetTemplateType(except);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of source mismatch type of except");
            }

            return CallQueryableMethod("Except", new[] { sourceType }, source, except);
        }

        #endregion

        #region ISkipExpressionMaker Members

        Expression ISkipExpressionMaker.Make(int count, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            //TODO: for sequence
            return CallQueryableMethod("Skip", new[] { GetTemplateType(source) }, source, Expression.Constant(count));
            //return QueryExpression.Skip(source, Expression.Constant(count));
        }

        #endregion

        #region ITakeExpressionMaker Members

        Expression ITakeExpressionMaker.Make(int count, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            //TODO: for sequence
            return CallQueryableMethod("Take", new[] { GetTemplateType(source)}, source, Expression.Constant(count));
            //return QueryExpression.Take(source, Expression.Constant(count));
        }

        #endregion

        #region IUnionExpressionMaker Members

        Expression IUnionExpressionMaker.Make(Expression source, Expression union)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(union, "union");

            Type sourceType = GetTemplateType(source);
            Type exceptType = GetTemplateType(union);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of source mismatch type of union");
            }

            return CallQueryableMethod("Union", new[] { sourceType }, source, union);
            //return QueryExpression.Except(source, except);
        }

        #endregion

        #region Implementation of IDistinctExpressionMaker

        Expression IDistinctExpressionMaker.Make(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod("Distinct", new[] { GetTemplateType(source) }, source);
        }

        #endregion

        #region Utility Methods

        internal static Type GetTemplateType(Expression sourceExpression)
        {
            const int templateIndex = 0;

            if (!sourceExpression.Type.IsGenericType)
            {
                return sourceExpression.Type;
            }

            Type[] genericArguments = sourceExpression.Type.GetGenericArguments();

            if (genericArguments.Length == 0)
            {
                throw new ArgumentException("sourceExpression is not generic source");
            }

            return genericArguments[templateIndex];
        }

        internal static Expression GetPropertyExpression(Type sourceType, string propertyName, Expression parameter)
        {
            PropertyInfo propertyInfo = sourceType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new SpoltyException(String.Format("property not found: {0}", propertyName));
            }

            return Expression.Property(parameter, propertyInfo);
        }

        internal static bool ContainsParameterExpression(Type type)
        {
            var parametersDictionary =
                (Dictionary<Type, ParameterExpression>)ThreadStorage.Current[ParameterDictionaryKey];
            if (parametersDictionary == null)
            {
                parametersDictionary = new Dictionary<Type, ParameterExpression>();
                ThreadStorage.Current[ParameterDictionaryKey] = parametersDictionary;
            }
            return parametersDictionary.ContainsKey(type);
        }

        internal static ParameterExpression GetParameterExpression(Type type, string name)
        {
            var parametersDictionary =
                (Dictionary<Type, ParameterExpression>) ThreadStorage.Current[ParameterDictionaryKey];
            if (parametersDictionary == null)
            {
                parametersDictionary = new Dictionary<Type, ParameterExpression>();
                ThreadStorage.Current[ParameterDictionaryKey] = parametersDictionary;
            }
            ParameterExpression result;
            parametersDictionary.TryGetValue(type, out result);
            if (result == null)
            {
                result = Expression.Parameter(type, name);
                parametersDictionary.Add(type, result);
            }
            return result;
        }

        internal static LambdaExpression GetLambdaExpression(Type sourceType, string includingProperty,
                                                             ParameterExpression parameter, MemberExpression member)
        {
            Expression property;
            if (member == null)
            {
                property = GetPropertyExpression(sourceType, includingProperty, parameter);
            }
            else
            {
                property =
                    GetPropertyExpression(ReflectionHelper.GetMemberType(member.Member), includingProperty, member);
            }
            return Expression.Lambda(property, parameter);
        }

        internal static Expression ConvertOrCastInnerPropertyExpression(Type outerPropertyType,
                                                                        PropertyInfo innerPropertyInfo,
                                                                        Expression innerPropertyExpression)
        {
            if (!innerPropertyInfo.PropertyType.IsGenericType)
            {
                if (IsConvertible(outerPropertyType))
                {
                    innerPropertyExpression = Expression.Convert(innerPropertyExpression, outerPropertyType);
                }
                else
                {
                    //Expression.Cast
                    innerPropertyExpression = Expression.TypeAs(innerPropertyExpression, outerPropertyType);
                }
            }
            return innerPropertyExpression;
        }

        private static bool IsConvertible(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof (Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        protected internal static Expression CallQueryableMethod(String methodName, Type[] typeArguments,
                                                                 params Expression[] arguments)
        {
            return Expression.Call(QueryableType, methodName, typeArguments, arguments);
        }

        protected internal static Expression CallEnumerableMethod(String methodName, Type[] typeArguments,
                                                                  params Expression[] arguments)
        {
            return Expression.Call(EnumerableType, methodName, typeArguments, arguments);
        }

        #endregion
    }
}