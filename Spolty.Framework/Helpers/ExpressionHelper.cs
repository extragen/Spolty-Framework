using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;

namespace Spolty.Framework.Helpers
{
    public static class ExpressionHelper
    {
        public static Expression CreateMemberExpression(Type sourceType, string memberName, Expression parameter, params Type[] interfaceRestriction)
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

        public static Type GetGenericType(Expression sourceExpression)
        {
            return ReflectionHelper.GetGenericType(sourceExpression.Type);
        }

        public static Type EnumerableType = typeof (Enumerable);
        public static Type IEnumerableType = typeof (IEnumerable);
        public static Type QueryableType = typeof (Queryable);
        public static Type GenericIEnumerableType = typeof (IEnumerable<>);
        public static readonly Type StringType = typeof(String);
        public static readonly Type IQueryableType = typeof (IQueryable);

        public static Expression CallQueryableMethod(String methodName, Type[] typeArguments,
                                                       params Expression[] arguments)
        {
            return Expression.Call(QueryableType, methodName, typeArguments, arguments);
        }

        public static Expression CallEnumerableMethod(String methodName, Type[] typeArguments,
                                                        params Expression[] arguments)
        {
            return Expression.Call(EnumerableType, methodName, typeArguments, arguments);
        }

        public static Expression CallMethod(string methodName, Expression sourceExpression, Expression conditionExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");

            Expression resultExpression = null;

            Type sourceType = GetGenericType(sourceExpression);
            if (ReflectionHelper.IsImplementingInterface(sourceExpression.Type, IQueryableType))
            {
                resultExpression = conditionExpression == null
                                       ? CallQueryableMethod(methodName, new[] {sourceType}, sourceExpression)
                                       : CallQueryableMethod(methodName, new[] {sourceType}, sourceExpression,
                                                             conditionExpression.NodeType == ExpressionType.Constant
                                                                 ? conditionExpression
                                                                 : Expression.Quote(conditionExpression));
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

        public static Expression ConvertOrCastInnerPropertyExpression(Type outerPropertyType,
                                                                        PropertyInfo innerPropertyInfo,
                                                                        Expression innerPropertyExpression)
        {
            innerPropertyExpression = ReflectionHelper.IsConvertible(outerPropertyType)
                                          ? Expression.Convert(innerPropertyExpression, outerPropertyType)
                                          : Expression.TypeAs(innerPropertyExpression, outerPropertyType);
            return innerPropertyExpression;
        }

        public const string ParameterDictionaryKey = "ParameterDictionary";

        public static ParameterExpression CreateOrGetParameterExpression(Type type, string name, Dictionary<string, object> store)
        {
            object storedValue;
            Dictionary<string, object> dictionary = store;
            if (!dictionary.TryGetValue(ParameterDictionaryKey, out storedValue))
            {
                storedValue = new Dictionary<Type, ParameterExpression>();
                dictionary.Add(ParameterDictionaryKey, storedValue);
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
    }
}
