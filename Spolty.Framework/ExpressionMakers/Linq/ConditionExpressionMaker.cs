using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Aggregations;
using Spolty.Framework.Parameters.Aggregations.Enums;
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

        public Expression Make(
            Expression sourceExpression,
            IEnumerable<BaseCondition> conditionals)
        {
            if (conditionals == null)
            {
                throw new ArgumentNullException("conditionals");
            }

            Type sourceType = GetTemplateType(sourceExpression);
            ParameterExpression p = GetParameterExpression(sourceType, sourceType.Name);
            foreach (BaseCondition condItem in conditionals)
            {
                Expression body = MakeSingleCondition(condItem, p, ref sourceExpression);

                if (body == null)
                {
                    continue;
                }

                sourceExpression = MakeWhere(sourceType, sourceExpression, body, p);
            }

            return sourceExpression;
        }

        public Expression MakeAggregate(
            Expression sourceExpression,
            ParameterExpression parameter,
            ConditionList conditionals,
            bool singleItem)
        {
            if (conditionals == null)
            {
                throw new ArgumentNullException("conditionals");
            }

            Type sourceType = GetTemplateType(sourceExpression);
            foreach (AggregateCondition aggregateCondition in conditionals)
            {
                Type elementType = aggregateCondition.ElementType;
                if (sourceType.GetMember(elementType.Name).Length == 0)
                {
                    throw new SpoltyException("There are no members");
                }

                AggregateMethod aggregateMethod;
                Type valueType;
                if (String.IsNullOrEmpty(aggregateCondition.FieldName) &&
                    (aggregateCondition.AggregateMethodType == AggregateMethodType.Count ||
                     aggregateCondition.AggregateMethodType == AggregateMethodType.LongCount))
                {
                    aggregateMethod = new AggregateMethod(aggregateCondition.AggregateMethodType);
                    valueType = typeof (int);
                }
                else
                {
                    PropertyInfo propertyInfo = elementType.GetProperty(aggregateCondition.FieldName);
                    if (propertyInfo == null)
                    {
                        throw new SpoltyException("propertyInfo is not found");
                    }
                    aggregateMethod = new ComplicatedAggregateMethod(aggregateCondition.AggregateMethodType, propertyInfo);
                    valueType = propertyInfo.PropertyType;
                }

                Expression aggregateExpression = null;
//                    AggregateExpressionMaker.MakeAggregateMethodExpression(sourceExpression, elementType, aggregateMethod, parameter);

                if (aggregateExpression == null)
                {
                    return null;
                }

                Expression constantExpression =
                    Expression.Constant(Convert.ChangeType(aggregateCondition.Value, valueType), valueType);

                Expression body =
                    MakeComarisonExpression(aggregateExpression, constantExpression, aggregateCondition.Operator);


                sourceExpression = MakeWhere(sourceType, sourceExpression, body, parameter);
            }
            return sourceExpression;
        }

        #endregion

        private Expression MakeSingleCondition(
            BaseCondition condItem,
            ParameterExpression parameter,
            ref Expression sourceExpression)
        {
            Expression body = null;
            Condition cond;
            ParameterExpression pe;
            if (condItem is Condition)
            {
                cond = (Condition) condItem;
                Type sourceType = GetTemplateType(sourceExpression);

                if (cond.ElementType == null)
                {
                    pe = GetParameterExpression(sourceType, sourceType.Name);
                }
                else
                {
                    var condition = ((Condition) condItem);
                    pe = GetParameterExpression(condition.ElementType, condition.ElementType.Name);
                }
                body = MakeSimpleCondition(cond, pe, ref sourceExpression);
            }
            else if (condItem is BoolCondition)
            {
                body = Expression.Constant(((BoolCondition) condItem).Value);
            }
            else if (condItem is FieldCondition)
            {
                var fieldCondition = (FieldCondition) condItem;
                body = MakeSimpleCondition(fieldCondition);
            }
            else if (condItem is BiCondition || condItem.GetType().IsSubclassOf(typeof (BiCondition)))
            {
                var biCond = (BiCondition) condItem;

                Expression leftExpr = null;
                Expression rightExpr = null;

                if (biCond.LeftCondition == null && biCond.RightCondition == null)
                {
                    return null;
                }

                if (biCond.LeftCondition != null)
                {
                    if (!(biCond.LeftCondition is Condition) || ((Condition) biCond.LeftCondition).ElementType == null)
                    {
                        pe = parameter;
                    }
                    else
                    {
                        var leftCondition = ((Condition)biCond.LeftCondition);
                        pe = GetParameterExpression(leftCondition.ElementType, leftCondition.ElementType.Name);
                    }

                    if (biCond.LeftCondition is Condition)
                    {
                        leftExpr =
                            MakeSimpleCondition((Condition) biCond.LeftCondition, pe,
                                                ref sourceExpression);
                    }
                    else
                    {
                        leftExpr = MakeSingleCondition(biCond.LeftCondition, pe, ref sourceExpression);
                    }
                }

                if (biCond.RightCondition != null)
                {
                    if (!(biCond.RightCondition is Condition) || ((Condition)biCond.RightCondition).ElementType == null)
                    {
                        pe = parameter;
                    }
                    else
                    {
                        var rightCondition = ((Condition)biCond.RightCondition);
                        pe = GetParameterExpression(rightCondition.ElementType, rightCondition.ElementType.Name);
                    }

                    if (biCond.RightCondition is Condition)
                    {
                        rightExpr =
                            MakeSimpleCondition((Condition) biCond.RightCondition, pe,
                                                ref sourceExpression);
                    }
                    else
                    {
                        rightExpr =
                            MakeSingleCondition(biCond.RightCondition, pe, ref sourceExpression);
                    }
                }

                if (leftExpr == null && rightExpr == null)
                {
                    return null;
                }

                if (leftExpr == null)
                {
                    body = rightExpr;
                }
                else if (rightExpr == null)
                {
                    body = leftExpr;
                }
                else
                {
                    if (biCond is OrCondition)
                    {
                        body = Expression.Or(leftExpr, rightExpr);
                    }
                    else if (biCond is AndCondition)
                    {
                        body = Expression.And(leftExpr, rightExpr);
                    }
                }
            }
            return body;
        }

        private Expression MakeSimpleCondition(
            Condition cond,
            ParameterExpression param,
            ref Expression sourceExpression)
        {
            Expression body = null;
            const int mainPropertyIndex = 0;
            const int secondaryPropertyIndex = 1;
            string[] properties = cond.FieldName.Split('.');
            Type sourceType = GetTemplateType(sourceExpression);
            Type parameterType = GetTemplateType(param);

            Type workType = sourceType.Name == parameterType.Name ? sourceType : parameterType;
            PropertyInfo pi = workType.GetProperty(properties[mainPropertyIndex]);
                                 
            if (pi != null)
            {
                Expression ex1 = Expression.Property(param, pi);
                PropertyInfo piChild = pi;
                for (int index = secondaryPropertyIndex; index < properties.Length; index++)
                {
                    piChild = piChild.PropertyType.GetProperty(properties[index]);
                    if (piChild == null)
                    {
                        if (pi.PropertyType.GetInterface(typeof (IQueryable).Name) != null)
                        {
                            int previousIndex = index - 1;
                            var condition =
                                new Condition(cond.FieldName.Remove(0, properties[previousIndex].Length), cond.Value,
                                              cond.Operator);
                            var innerConditions = new ConditionList(condition);

                            IQueryable innerSource = Factory.GetTable(pi.PropertyType.GetGenericArguments()[0]);

                            Expression innerExpression = Make(innerSource.Expression,
                                                              innerConditions);

                            //TODO: new JoinExpressionMaker(Factory).MakeInnerJoin
                            sourceExpression = new JoinExpressionMaker(Factory).MakeInnerJoin(sourceExpression, innerExpression,
                                                                                              innerSource.ElementType,
                                                                                              sourceType);
                        }
                        return body;
                    }
                    ex1 = Expression.Property(ex1, piChild);
                }

                Expression ex2 = Expression.Constant(cond.Value, piChild.PropertyType);

                body = MakeComarisonExpression(ex1, ex2, cond.Operator);
            }

            return body;
        }

        private static Expression MakeSimpleCondition(FieldCondition fieldCondition)
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
                GetParameterExpression(fieldCondition.LeftElementType, fieldCondition.LeftElementType.Name);
            ParameterExpression rightParam =
                GetParameterExpression(fieldCondition.RightElementType, fieldCondition.RightElementType.Name);

            Expression leftExpression =
                GetPropertyExpression(fieldCondition.LeftElementType, fieldCondition.LeftFieldName, leftParam);
            Expression rightExpression =
                GetPropertyExpression(fieldCondition.RightElementType, fieldCondition.RightFieldName, rightParam);

            rightExpression =
                ConvertOrCastInnerPropertyExpression(leftPropertyInfo.PropertyType, rightPropertyInfo, rightExpression);

            Expression body = MakeComarisonExpression(leftExpression, rightExpression, fieldCondition.Operator);
            return body;
        }

        private static Expression MakeComarisonExpression(
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
                case ConditionOperator.GreaterOrEqualTo:
                    body = Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                    break;
                case ConditionOperator.GreaterThen:
                    body = Expression.GreaterThan(leftExpression, rightExpression);
                    break;
                case ConditionOperator.LessOrEqualTo:
                    body = Expression.LessThanOrEqual(leftExpression, rightExpression);
                    break;
                case ConditionOperator.LessThen:
                    body = Expression.LessThan(leftExpression, rightExpression);
                    break;
                case ConditionOperator.StartsWith:
                    body = Expression.Call(leftExpression,
                                           typeof(String).GetMethod("StartsWith", new[] { typeof(string) }),
                                           rightExpression);
                    break;
                case ConditionOperator.Like:
                    body = Expression.Call(leftExpression,
                                           typeof (String).GetMethod("Contains", new[] {typeof (string)}),
                                           rightExpression);
                    break;
                case ConditionOperator.EndsWith:
                    body = Expression.Call(leftExpression,
                                           typeof(String).GetMethod("EndsWith", new[] { typeof(string) }),
                                           rightExpression);
                    break;
            }
            return body;
        }

        private static Expression MakeWhere(
            Type sourceType,
            Expression source,
            Expression body,
            ParameterExpression parameter)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameter);

            return CallQueryableMethod("Where", new[] {sourceType}, source, Expression.Quote(lambdaExpression));
        }
    }
}