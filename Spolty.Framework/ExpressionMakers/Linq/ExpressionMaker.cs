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
        public static readonly Type StringType = typeof(String);
        public static readonly Type IQueryableType = typeof (IQueryable);

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

        Expression IExpressionMaker.MakeExcept(Expression sourceExpression, Expression exceptExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");
            Checker.CheckArgumentNull(exceptExpression, "exceptExpression");

            Type sourceType = GetGenericType(sourceExpression);
            Type exceptType = GetGenericType(exceptExpression);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of sourceExpression mismatch type of exceptExpression");
            }

            return CallQueryableMethod(MethodName.Except, new[] { sourceType }, sourceExpression, exceptExpression);
        }

        Expression IExpressionMaker.MakeSkip(int count, Expression sourceExpression)
        {
            return CallMethod(MethodName.Skip, sourceExpression, Expression.Constant(count));
        }

        Expression IExpressionMaker.MakeTake(int count, Expression sourceExpression)
        {
            return CallMethod(MethodName.Take, sourceExpression, Expression.Constant(count));
        }

        Expression IExpressionMaker.MakeUnion(Expression sourceExpression, Expression unionExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");
            Checker.CheckArgumentNull(unionExpression, "unionExpression");

            Type sourceType = GetGenericType(sourceExpression);
            Type exceptType = GetGenericType(unionExpression);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of sourceExpression mismatch type of unionExpression");
            }

            return CallQueryableMethod(MethodName.Union, new[] { sourceType }, sourceExpression, unionExpression);
        }

        Expression IExpressionMaker.MakeDistinct(Expression sourceExpression)
        {
            return CallMethod(MethodName.Distinct, sourceExpression, null);
        }

        Expression IExpressionMaker.MakeAny(Expression sourceExpression, Expression conditionExpression)
        {
            return CallMethod(MethodName.Any, sourceExpression, conditionExpression);
        }

        #region Implementation of IExpressionMaker

        public Expression MakeAll(Expression sourceExpression, Expression conditionExpression)
        {
            return CallMethod(MethodName.All, sourceExpression, conditionExpression);
        }

        #endregion

        Expression IExpressionMaker.MakeCount(Expression sourceExpression, Expression conditionExpression)
        {
            return CallMethod(MethodName.Count, sourceExpression, conditionExpression);
        }

        Expression IExpressionMaker.MakeFirst(Expression sourceExpression)
        {
            return CallMethod(MethodName.First, sourceExpression, null);
        }

        Expression IExpressionMaker.MakeFirstOrDefault(Expression sourceExpression)
        {
            return CallMethod(MethodName.FirstOrDefault, sourceExpression, null);
        }

        #endregion

        #region Utility Methods

        internal static Type GetGenericType(Expression sourceExpression)
        {
            return ReflectionHelper.GetGenericType(sourceExpression.Type);
        }

        internal static Expression CreateMemberExpression(Type sourceType, string memberName, Expression parameter, params Type[] interfaceRestriction)
        {
            MemberInfo memberInfo = ReflectionHelper.GetMemberInfo(sourceType, memberName);
            if (memberInfo == null)
            {
                throw new SpoltyException(String.Format("member not found: {0}", memberName));
            }

            if (interfaceRestriction != null)
            {
                Type memberType = ReflectionHelper.GetMemberType(memberInfo);
                foreach (var restriction in interfaceRestriction)
                {
                    if (!ReflectionHelper.IsImplementingInterface(memberType, restriction))
                    {
                        throw new SpoltyException(String.Format("memberInfo not implement interface {0}", restriction));
                    }
                }
            }

            MemberExpression memberExpression;

            if (memberInfo is FieldInfo)
            {
                memberExpression = Expression.Field(parameter, (FieldInfo) memberInfo);
            }
            else if (memberInfo is PropertyInfo)
            {
                memberExpression = Expression.Property(parameter, (PropertyInfo) memberInfo);
            }
            else
            {
                throw new SpoltyException("memberInfo not supported.");
            }

            if (memberExpression == null)
            {
                throw new SpoltyException("memberExpression not created.");
            }

            return memberExpression;
        }

        internal ParameterExpression CreateOrGetParameterExpression(Type type, string name)
        {
            object storedValue;
            if (!Factory.Store.TryGetValue(ParameterDictionaryKey, out storedValue))
            {
                storedValue = new Dictionary<Type, ParameterExpression>();
                Factory.Store.Add(ParameterDictionaryKey, storedValue);
            }

            var parametersDictionary = (Dictionary<Type, ParameterExpression>) storedValue;

            ParameterExpression result;
            parametersDictionary.TryGetValue(type, out result);
            if (result == null)
            {
                result = Expression.Parameter(type, name);
                parametersDictionary.Add(type, result);
            }
            return result;
        }

        internal static LambdaExpression CreateLambdaExpression(Type sourceType, string includingMember,
                                                             ParameterExpression parameter, MemberExpression member)
        {
            Expression memberExpression = member == null
                                              ? CreateMemberExpression(sourceType, includingMember, parameter)
                                              : CreateMemberExpression(ReflectionHelper.GetMemberType(member.Member),
                                                                       includingMember, member);
            return Expression.Lambda(memberExpression, parameter);
        }

        internal static Expression ConvertOrCastInnerPropertyExpression(Type outerPropertyType,
                                                                        PropertyInfo innerPropertyInfo,
                                                                        Expression innerPropertyExpression)
        {
            if (!innerPropertyInfo.PropertyType.IsGenericType)
            {
                innerPropertyExpression = IsConvertible(outerPropertyType)
                                              ? Expression.Convert(innerPropertyExpression, outerPropertyType)
                                              : Expression.TypeAs(innerPropertyExpression, outerPropertyType);
            }
            return innerPropertyExpression;
        }

        internal static bool IsConvertible(Type type)
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

        protected static Expression CallMethod(string methodName, Expression sourceExpression, Expression conditionExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");

            Expression resultExpression = null;

            Type sourceType = GetGenericType(sourceExpression);
            if (ReflectionHelper.IsImplementingInterface(sourceExpression.Type, IQueryableType))
            {
                resultExpression = conditionExpression == null
                                       ? CallQueryableMethod(methodName, new[] {sourceType}, sourceExpression)
                                       : CallQueryableMethod(methodName, new[] {sourceType}, sourceExpression,
                                                             Expression.Quote(conditionExpression));
            }
            else if (ReflectionHelper.IsImplementingInterface(sourceExpression.Type, IEnumerableType))
            {
                resultExpression = conditionExpression == null
                                       ? CallEnumerableMethod(methodName, new[] {sourceType}, sourceExpression)
                                       : CallEnumerableMethod(methodName, new[] {sourceType}, sourceExpression,
                                                              conditionExpression);
            }

            if (resultExpression == null)
            {
                throw new SpoltyException("sourceExpression not implemented any required interfaces.");
            }

            return resultExpression;
        }

        #endregion
    }
}