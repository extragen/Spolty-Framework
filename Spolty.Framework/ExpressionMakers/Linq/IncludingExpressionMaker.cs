using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;
using Spolty.Framework.Parameters.BaseNode;
using Spolty.Framework.Parameters.Loads;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class IncludingExpressionMaker : ExpressionMaker
    {
        public IncludingExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        public List<LambdaExpression> MakeIncluding(Type sourceType, Expression source, LoadNode rootNode)
        {
            if (rootNode.ParentNode != null && !rootNode.Including)
            {
                return null;
            }

            ParameterExpression parameter = CreateOrGetParameterExpression(sourceType, sourceType.Name);

            MemberExpression memberExpression = null;
            if (rootNode.ParentNode == null &&
                !rootNode.Including &&
                sourceType != rootNode.EntityType)
            {
                var memberInfos =
                    new List<MemberInfo>(sourceType.FindMembers(MemberTypes.Field | MemberTypes.Property,
                                                                BindingFlags.Instance | BindingFlags.Public, Filter,
                                                                rootNode));
                const int numberOfAcceptableMembers = 1;
                if (memberInfos.Count != numberOfAcceptableMembers)
                {
                    throw new SpoltyException("Number of members not correct");
                }
                MemberInfo memberInfo = memberInfos[0];
                if (memberInfo == null)
                {
                    throw new SpoltyException(String.Format("Member {0} not found", rootNode.EntityType.Name));
                }
                if (memberInfo is PropertyInfo)
                {
                    memberExpression = Expression.Property(parameter, (PropertyInfo) memberInfo);
                }
                else if (memberInfo is FieldInfo)
                {
                    memberExpression = Expression.Field(parameter, (FieldInfo) memberInfo);
                }
                else
                {
                    throw new SpoltyException("Type not supported");
                }
            }

            var expressionList = new List<LambdaExpression>();

            foreach (LoadNode childNode in rootNode.ChildNodes)
            {
                if (!childNode.Including)
                {
                    continue;
                }

                var newExpression = new List<LambdaExpression>();
                if (childNode.ChildNodes.Count > 0)
                {
                    Expression property;
                    if (memberExpression == null)
                    {
                        property = CreatePropertyExpression(sourceType, childNode.PropertyName, parameter);
                    }
                    else
                    {
                        property =
                            CreatePropertyExpression(childNode.ParentNode.EntityType, childNode.PropertyName,
                                                  memberExpression);
                    }
                    newExpression = MakeIncluding(childNode.EntityType, property, childNode);
                }
                else
                {
                    newExpression.Add(CreateLambdaExpression(sourceType, childNode.PropertyName, parameter,
                                                          memberExpression));
                }
                expressionList.AddRange(newExpression);
            }

            return expressionList;
        }

        public List<LambdaExpression> MakeIncluding(Expression source, params Type[] includingTypes)
        {
            Type sourceType = GetGenericType(source);
            ParameterExpression parameter = CreateOrGetParameterExpression(sourceType, sourceType.Name);
            var resultExpression = new List<LambdaExpression>();
            foreach (Type includingType in includingTypes)
            {
                LambdaExpression lambdaExpression = CreateLambdaExpression(sourceType, includingType.Name, parameter, null);
                resultExpression.Add(lambdaExpression);
            }
            return resultExpression;
        }

        private static bool Filter(MemberInfo mi, object filterCriteria)
        {
            return ReflectionHelper.GetMemberType(mi) == ((BaseNode) filterCriteria).EntityType;
        }
    }
}