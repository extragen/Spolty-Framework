using System;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.ExpressionMakers.Linq;
using Spolty.Framework.Parameters.Joins;
using DynamicExpression=System.Linq.Dynamic.DynamicExpression;
using DynamicProperty=System.Linq.Dynamic.DynamicProperty;

namespace Spolty.Framework.ExpressionMakers.EntityFramework
{
    internal class EFJoinMaker : JoinExpressionMaker
    {
        public EFJoinMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        protected override Expression MakeLeftJoin(Expression outerSourceExpression, Expression innerSourceExpression,
                                                   Type unitingTyped, Type resultSelectorType, JoinNode childNode)
        {
            Type outerType = GetGenericType(outerSourceExpression);
            Type innerType = GetGenericType(innerSourceExpression);

            ParameterExpression outerParam = CreateOrGetParameterExpression(outerType, outerType.Name);
            
            Expression innerMemberEx = Expression.PropertyOrField(outerParam, innerType.Name);

            if (childNode.Conditions.Count > 0)
            {
                innerMemberEx = Factory.CreateConditionExpressionMaker().Make(childNode.Conditions, innerMemberEx);
            }

            var outerProperty = new DynamicProperty(outerType.Name, outerType);
            var innerProperty = new DynamicProperty(innerType.Name, innerMemberEx.Type);

            Type dynamicClassType = DynamicExpression.CreateClass(outerProperty, innerProperty);

            PropertyInfo outerDynamicProperty = dynamicClassType.GetProperty(outerType.Name);
            PropertyInfo innerDynamicProperty = dynamicClassType.GetProperty(innerType.Name);
            
            var bindings = new MemberBinding[2];
            bindings[0] = Expression.Bind(outerDynamicProperty, outerParam);
            bindings[1] = Expression.Bind(innerDynamicProperty, innerMemberEx);

            Expression newExpression = Expression.MemberInit(Expression.New(dynamicClassType), bindings);
            LambdaExpression resultSelector = Expression.Lambda(newExpression, outerParam);

            Expression selectNewQueryableExpression = CallQueryableMethod(MethodName.Select,
                                                                          new[] { outerType, resultSelector.Body.Type },
                                                                          outerSourceExpression,
                                                                          Expression.Quote(resultSelector));

            ParameterExpression dynParam = CreateOrGetParameterExpression(dynamicClassType, dynamicClassType.Name);

            resultSelector = Expression.Lambda(Expression.Property(dynParam, outerDynamicProperty), dynParam);

            Expression leftJoinExpression = CallQueryableMethod(MethodName.Select,
                                                                new[] { GetGenericType(selectNewQueryableExpression), resultSelector.Body.Type }, 
                                                                selectNewQueryableExpression,
                                                                Expression.Quote(resultSelector));

            return selectNewQueryableExpression;
            return leftJoinExpression;
        }
    }
}