using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal sealed class ConditionExpressionMaker : ExpressionMaker, IConditionExpressionMaker
    {
        public ConditionExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        #region IConditionExpressionMaker Members

        public Expression Make(IEnumerable<BaseCondition> conditions, Expression sourceExpression)
        {
            Checker.CheckArgumentNull(conditions, "conditionals");
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");

            Type sourceType = GetGenericType(sourceExpression);
            ParameterExpression parameterExpression = CreateOrGetParameterExpression(sourceType, sourceType.Name);
            AndCondition condition = new AndCondition(conditions);

            Expression body = MakeConditionExpression(condition, parameterExpression, ref sourceExpression);
            sourceExpression = MakeWhere(sourceType, sourceExpression, body, parameterExpression);

            return sourceExpression;
        }

        #endregion

        private Expression MakeConditionExpression(
            BaseCondition baseCondition,
            ParameterExpression parameter,
            ref Expression sourceExpression)
        {
            Expression body = null;
            ParameterExpression pe;
            if (baseCondition is Condition)
            {
                Condition condition = (Condition) baseCondition;
                Type sourceType = GetGenericType(sourceExpression);

                pe = condition.ElementType == null
                         ? CreateOrGetParameterExpression(sourceType, sourceType.Name)
                         : CreateOrGetParameterExpression(condition.ElementType, condition.ElementType.Name);

                body = MakeSimpleCondition(condition, pe, ref sourceExpression);
            }
            else if (baseCondition is BoolCondition)
            {
                body = Expression.Constant(((BoolCondition) baseCondition).Value);
            }
            else if (baseCondition is FieldCondition)
            {
                var fieldCondition = (FieldCondition) baseCondition;
                body = MakeSimpleCondition(fieldCondition);
            }
            else if (baseCondition is BiCondition || baseCondition.GetType().IsSubclassOf(typeof (BiCondition)))
            {
                var biCondition = (BiCondition) baseCondition;

                Expression leftExpression = null;
                Expression rightExpression = null;

                if (biCondition.LeftCondition == null && biCondition.RightCondition == null)
                {
                    return null;
                }

                if (biCondition.LeftCondition != null)
                {
                    if (!(biCondition.LeftCondition is Condition) || ((Condition) biCondition.LeftCondition).ElementType == null)
                    {
                        pe = parameter;
                    }
                    else
                    {
                        var leftCondition = ((Condition)biCondition.LeftCondition);
                        pe = CreateOrGetParameterExpression(leftCondition.ElementType, leftCondition.ElementType.Name);
                    }

                    if (biCondition.LeftCondition is Condition)
                    {
                        leftExpression =
                            MakeSimpleCondition((Condition) biCondition.LeftCondition, pe,
                                                ref sourceExpression);
                    }
                    else
                    {
                        leftExpression = MakeConditionExpression(biCondition.LeftCondition, pe, ref sourceExpression);
                    }
                }

                if (biCondition.RightCondition != null)
                {
                    if (!(biCondition.RightCondition is Condition) || ((Condition)biCondition.RightCondition).ElementType == null)
                    {
                        pe = parameter;
                    }
                    else
                    {
                        var rightCondition = ((Condition)biCondition.RightCondition);
                        pe = CreateOrGetParameterExpression(rightCondition.ElementType, rightCondition.ElementType.Name);
                    }

                    if (biCondition.RightCondition is Condition)
                    {
                        rightExpression =
                            MakeSimpleCondition((Condition) biCondition.RightCondition, pe,
                                                ref sourceExpression);
                    }
                    else
                    {
                        rightExpression =
                            MakeConditionExpression(biCondition.RightCondition, pe, ref sourceExpression);
                    }
                }

                if (leftExpression == null && rightExpression == null)
                {
                    return null;
                }

                if (leftExpression == null)
                {
                    body = rightExpression;
                }
                else if (rightExpression == null)
                {
                    body = leftExpression;
                }
                else
                {
                    if (biCondition is OrCondition)
                    {
                        body = Expression.Or(leftExpression, rightExpression);
                    }
                    else if (biCondition is AndCondition)
                    {
                        body = Expression.And(leftExpression, rightExpression);
                    }
                }
            }
            else if (baseCondition is PredicateAggregationCondition)
            {
                PredicateAggregationCondition predicateAggregationCondition = (PredicateAggregationCondition) baseCondition;
                Type sourceType = GetGenericType(sourceExpression);
                if (predicateAggregationCondition.ElementType != null && 
                    predicateAggregationCondition.ElementType != sourceType)
                {
                    throw new SpoltyException(
                        String.Format("Aggregation condition with ElementType {0} can not assign to queryable ElementType {1}",
                                      predicateAggregationCondition.ElementType, sourceType));
                }
                
                var memberExpression = CreateMemberExpression(sourceType, predicateAggregationCondition.EnumerableFieldName, parameter, IEnumerableType);
                Expression conditionExpression = null;
                if (predicateAggregationCondition.Conditions.Count > 0)
                {
                    Type memberType = GetGenericType(memberExpression);
                    predicateAggregationCondition.Conditions.SetElementType(memberType);
                    conditionExpression = MakeConditionExpression(new AndCondition(predicateAggregationCondition.Conditions), parameter,
                                                   ref memberExpression);
                    conditionExpression = Expression.Lambda(conditionExpression, CreateOrGetParameterExpression(memberType, memberType.Name));
                }

                switch (predicateAggregationCondition.AggregationMethod)
                {
                    case AggregationMethod.All:
                        body = Factory.CreateExpressionMaker().MakeAll(memberExpression, conditionExpression);
                        break;
                    case AggregationMethod.Any:
                        body = Factory.CreateExpressionMaker().MakeAny(memberExpression, conditionExpression);
                        break;
                    case AggregationMethod.Average:
                        break;
                    case AggregationMethod.Count:
                        body = Factory.CreateExpressionMaker().MakeCount(memberExpression, conditionExpression);
                        break;
                    case AggregationMethod.Max:
                        break;
                    case AggregationMethod.Min:
                        break;
                    case AggregationMethod.Sum:
                        break;
                    default:
                        throw new SpoltyException("Not supported AggregationMethod.");
                }

                body = MakeComparisonExpression(body, Expression.Constant(predicateAggregationCondition.Value),
                                                predicateAggregationCondition.Operator);

            }
            return body;
        }

        private Expression MakeSimpleCondition(
            Condition condition,
            Expression parameterExpression,
            ref Expression sourceExpression)
        {
            Expression body = null;
            const int mainPropertyIndex = 0;
            const int secondaryPropertyIndex = 1;
            string[] properties = condition.FieldName.Split('.');
            Type sourceType = GetGenericType(sourceExpression);
            Type parameterType = GetGenericType(parameterExpression);

            Type workType = sourceType.Name == parameterType.Name ? sourceType : parameterType;
            PropertyInfo propertyInfo = workType.GetProperty(properties[mainPropertyIndex]);
                                 
            if (propertyInfo != null)
            {
                Expression memberExpression = Expression.Property(parameterExpression, propertyInfo);
                PropertyInfo childPropertyInfo = propertyInfo;
                for (int index = secondaryPropertyIndex; index < properties.Length; index++)
                {
                    if (childPropertyInfo != null)
                    {
                        childPropertyInfo = childPropertyInfo.PropertyType.GetProperty(properties[index]);
                    }

                    if (childPropertyInfo == null)
                    {
                        if (propertyInfo.PropertyType.GetInterface(IQueryableType.Name) != null)
                        {
                            int previousIndex = index - 1;
                            var innerCondition =
                                new Condition(condition.FieldName.Remove(0, properties[previousIndex].Length), condition.Value,
                                              condition.Operator);
                            var innerConditions = new ConditionList(innerCondition);

                            IQueryable innerSource = Factory.GetTable(ReflectionHelper.GetGenericType(propertyInfo.PropertyType));

                            Expression innerExpression = Make(innerConditions, innerSource.Expression);

                            //TODO: new JoinExpressionMaker(Factory).MakeInnerJoin
                            sourceExpression = new JoinExpressionMaker(Factory).MakeInnerJoin(sourceExpression, innerExpression,
                                                                                              innerSource.ElementType,
                                                                                              sourceType);
                        }

                        return body;
                    }
                    memberExpression = Expression.Property(memberExpression, childPropertyInfo);
                }

                Expression constantExpression;
                Type valueType = condition.Value.GetType();
                Type propertyType = ReflectionHelper.GetGenericType(childPropertyInfo.PropertyType);
                if (valueType == propertyType || !IsConvertible(propertyType))
                {
                    constantExpression = Expression.Constant(condition.Value, childPropertyInfo.PropertyType);
                }
                else
                {
                    constantExpression = Expression.Constant(Convert.ChangeType(condition.Value, propertyType),
                                                             childPropertyInfo.PropertyType);
                }

                body = MakeComparisonExpression(memberExpression, constantExpression, condition.Operator);
            }

            return body;
        }

        private Expression MakeSimpleCondition(FieldCondition fieldCondition)
        {
            PropertyInfo leftPropertyInfo = fieldCondition.LeftElementType.GetProperty(fieldCondition.LeftFieldName);
            if (leftPropertyInfo == null)
            {
                throw new SpoltyException("Left field not found");
            }

            PropertyInfo rightPropertyInfo = fieldCondition.RightElementType.GetProperty(fieldCondition.RightFieldName);
            if (rightPropertyInfo == null)
            {
                throw new SpoltyException("Right field not found");
            }

            ParameterExpression leftParam =
                CreateOrGetParameterExpression(fieldCondition.LeftElementType, fieldCondition.LeftElementType.Name);
            ParameterExpression rightParam =
                CreateOrGetParameterExpression(fieldCondition.RightElementType, fieldCondition.RightElementType.Name);

            Expression leftExpression =
                CreateMemberExpression(fieldCondition.LeftElementType, fieldCondition.LeftFieldName, leftParam);
            Expression rightExpression =
                CreateMemberExpression(fieldCondition.RightElementType, fieldCondition.RightFieldName, rightParam);

            rightExpression =
                ConvertOrCastInnerPropertyExpression(leftPropertyInfo.PropertyType, rightPropertyInfo, rightExpression);

            Expression body = MakeComparisonExpression(leftExpression, rightExpression, fieldCondition.Operator);
            return body;
        }

        private static Expression MakeComparisonExpression(
            Expression leftExpression,
            Expression rightExpression,
            ConditionOperator conditionOperator)
        {
            Expression body = null;
            switch (conditionOperator)
            {
                case ConditionOperator.EqualTo:
                    body = Expression.Equal(leftExpression, rightExpression);
                    break;
                case ConditionOperator.NotEqualTo:
                    body = Expression.NotEqual(leftExpression, rightExpression);
                    break;
                case ConditionOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                    break;
                case ConditionOperator.GreaterThan:
                    body = Expression.GreaterThan(leftExpression, rightExpression);
                    break;
                case ConditionOperator.LessThanOrEqualTo:
                    body = Expression.LessThanOrEqual(leftExpression, rightExpression);
                    break;
                case ConditionOperator.LessThan:
                    body = Expression.LessThan(leftExpression, rightExpression);
                    break;
                case ConditionOperator.StartsWith:
                    body = Expression.Call(leftExpression,
                                           StringType.GetMethod(MethodName.StartsWith, new[] { StringType }),
                                           rightExpression);
                    break;
                case ConditionOperator.Like:
                    body = Expression.Call(leftExpression,
                                           StringType.GetMethod(MethodName.Contains, new[] { StringType }),
                                           rightExpression);
                    break;
                case ConditionOperator.EndsWith:
                    body = Expression.Call(leftExpression,
                                           StringType.GetMethod(MethodName.EndsWith, new[] { StringType }),
                                           rightExpression);
                    break;
            }
            return body;
        }

        private static Expression MakeWhere(
            Type sourceType,
            Expression sourceExpression,
            Expression bodyExpression,
            ParameterExpression parameterExpression)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(bodyExpression, parameterExpression);
            Expression resultExpression = null;
            
            if (ReflectionHelper.IsImplementingInterface(sourceExpression.Type, IQueryableType))
            {
                resultExpression = CallQueryableMethod(MethodName.Where, new[] {sourceType}, sourceExpression, Expression.Quote(lambdaExpression));
            }
            else if (ReflectionHelper.IsImplementingInterface(sourceExpression.Type, IEnumerableType))
            {
                resultExpression = CallEnumerableMethod(MethodName.Where, new[] {sourceType}, sourceExpression, lambdaExpression);
            }
            
            if (resultExpression == null)
            {
                throw new SpoltyException("sourceExpression not implemented any required interfaces/");
            }

            return resultExpression;
        }
    }
}