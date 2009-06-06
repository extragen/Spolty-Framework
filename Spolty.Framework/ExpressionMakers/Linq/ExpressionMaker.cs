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

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class ExpressionMaker : IExpressionMaker
    {
        private const string ParameterDictionaryKey = "ParameterDictionary";

        protected static Type EnumerableType;
        protected static Type IEnumerableType;
        protected static Type QueryableType;
        private readonly IExpressionMakerFactory _factory;
        protected static Type GenericIEnumerableType; 

        #region Constructors

        static ExpressionMaker()
        {
            QueryableType = typeof (Queryable);
            EnumerableType = typeof (Enumerable);
            IEnumerableType = typeof (IEnumerable);
            GenericIEnumerableType = typeof (IEnumerable<>);
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

        Expression IExpressionMaker.MakeExcept(Expression source, Expression except)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(except, "except");

            Type sourceType = GetGenericType(source);
            Type exceptType = GetGenericType(except);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of source mismatch type of except");
            }

            return CallQueryableMethod(MethodName.Except, new[] { sourceType }, source, except);
        }

        Expression IExpressionMaker.MakeSkip(int count, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod(MethodName.Skip, new[] { GetGenericType(source) }, source, Expression.Constant(count));
        }

        Expression IExpressionMaker.MakeTake(int count, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            //TODO: for sequence
            return CallQueryableMethod(MethodName.Take, new[] { GetGenericType(source)}, source, Expression.Constant(count));
            //return QueryExpression.Take(source, Expression.Constant(count));
        }

        Expression IExpressionMaker.MakeUnion(Expression source, Expression union)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(union, "union");

            Type sourceType = GetGenericType(source);
            Type exceptType = GetGenericType(union);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of source mismatch type of union");
            }

            return CallQueryableMethod(MethodName.Union, new[] { sourceType }, source, union);
            //return QueryExpression.Except(source, except);
        }

        Expression IExpressionMaker.MakeDistinct(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod(MethodName.Distinct, new[] { GetGenericType(source) }, source);
        }

        Expression IExpressionMaker.MakeAny(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod(MethodName.Any, new[] {GetGenericType(source)}, source);
        }

        Expression IExpressionMaker.MakeCount(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");
           
            return CallQueryableMethod(MethodName.Count, new[] { GetGenericType(source) }, source);
        }

        Expression IExpressionMaker.MakeFirst(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod(MethodName.First, new[] { GetGenericType(source) }, source);
        }

        Expression IExpressionMaker.MakeFirstOrDefault(Expression source)
        {
            Checker.CheckArgumentNull(source, "source");

            return CallQueryableMethod(MethodName.FirstOrDefault, new[] { GetGenericType(source) }, source);
        }

        #endregion

        #region Utility Methods

        internal static Type GetGenericType(Expression sourceExpression)
        {
            const int genericIndex = 0;

            if (!sourceExpression.Type.IsGenericType)
            {
                return sourceExpression.Type;
            }

            Type[] genericArguments = sourceExpression.Type.GetGenericArguments();

            if (genericArguments.Length == 0)
            {
                throw new ArgumentException("sourceExpression is not generic source");
            }

            return genericArguments[genericIndex];
        }

        internal static Expression CreatePropertyExpression(Type sourceType, string propertyName, Expression parameter)
        {
            PropertyInfo propertyInfo = sourceType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new SpoltyException(String.Format("property not found: {0}", propertyName));
            }

            return Expression.Property(parameter, propertyInfo);
        }

        internal ParameterExpression CreateOrGetParameterExpression(Type type, string name)
        {
            object storeValue;
            if (!Factory.Store.TryGetValue(ParameterDictionaryKey, out storeValue))
            {
                storeValue = new Dictionary<Type, ParameterExpression>();
                Factory.Store.Add(ParameterDictionaryKey, storeValue);
            }

            var parametersDictionary = (Dictionary<Type, ParameterExpression>) storeValue;

            ParameterExpression result;
            parametersDictionary.TryGetValue(type, out result);
            if (result == null)
            {
                result = Expression.Parameter(type, name);
                parametersDictionary.Add(type, result);
            }
            return result;
        }

        internal static LambdaExpression CreateLambdaExpression(Type sourceType, string includingProperty,
                                                             ParameterExpression parameter, MemberExpression member)
        {
            Expression property;
            if (member == null)
            {
                property = CreatePropertyExpression(sourceType, includingProperty, parameter);
            }
            else
            {
                property =
                    CreatePropertyExpression(ReflectionHelper.GetMemberType(member.Member), includingProperty, member);
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