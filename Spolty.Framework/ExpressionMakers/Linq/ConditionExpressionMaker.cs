using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal sealed class ConditionExpressionMaker : ExpressionMaker, IConditionExpressionMaker
    {
        private static readonly Type StringType = typeof(String);
        private static readonly Type IQueryableType = typeof (IQueryable);

        public ConditionExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        #region IConditionExpressionMaker Members

        public Expression Make(IEnumerable<BaseCondition> conditionals, Expression sourceExpression)
        {
            Checker.CheckArgumentNull(conditionals, "conditionals");
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");

            Type sourceType = GetGenericType(sourceExpression);
            ParameterExpression parameterExpression = CreateOrGetParameterExpression(sourceType, sourceType.Name);
            foreach (BaseCondition baseCondition in conditionals)
            {
                Expression body = MakeConditionExpression(baseCondition, parameterExpression, ref sourceExpression);

                if (body == null)
                {
                    continue;
                }

                sourceExpression = MakeWhere(sourceType, sourceExpression, body, parameterExpression);
            }

            return sourceExpression;
        }

        #endregion

        private Expression MakeConditionExpression(
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
                Type sourceType = GetGenericType(sourceExpression);

                if (cond.ElementType == null)
                {
                    pe = CreateOrGetParameterExpression(sourceType, sourceType.Name);
                }
                else
                {
                    var condition = ((Condition) condItem);
                    pe = CreateOrGetParameterExpression(condition.ElementType, condition.ElementType.Name);
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
                        pe = CreateOrGetParameterExpression(leftCondition.ElementType, leftCondition.ElementType.Name);
                    }

                    if (biCond.LeftCondition is Condition)
                    {
                        leftExpr =
                            MakeSimpleCondition((Condition) biCond.LeftCondition, pe,
                                                ref sourceExpression);
                    }
                    else
                    {
                        leftExpr = MakeConditionExpression(biCond.LeftCondition, pe, ref sourceExpression);
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
                        pe = CreateOrGetParameterExpression(rightCondition.ElementType, rightCondition.ElementType.Name);
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
                            MakeConditionExpression(biCond.RightCondition, pe, ref sourceExpression);
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
            Expression param,
            ref Expression sourceExpression)
        {
            Expression body = null;
            const int mainPropertyIndex = 0;
            const int secondaryPropertyIndex = 1;
            string[] properties = cond.FieldName.Split('.');
            Type sourceType = GetGenericType(sourceExpression);
            Type parameterType = GetGenericType(param);

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
                        if (pi.PropertyType.GetInterface(IQueryableType.Name) != null)
                        {
                            int previousIndex = index - 1;
                            var condition =
                                new Condition(cond.FieldName.Remove(0, properties[previousIndex].Length), cond.Value,
                                              cond.Operator);
                            var innerConditions = new ConditionList(condition);

                            IQueryable innerSource = Factory.GetTable(pi.PropertyType.GetGenericArguments()[0]);

                            Expression innerExpression = Make(innerConditions, innerSource.Expression);

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
                CreatePropertyExpression(fieldCondition.LeftElementType, fieldCondition.LeftFieldName, leftParam);
            Expression rightExpression =
                CreatePropertyExpression(fieldCondition.RightElementType, fieldCondition.RightFieldName, rightParam);

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
            Expression source,
            Expression body,
            ParameterExpression parameter)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameter);
            Expression resultExpression = null;
            if (source.Type.GetInterface(IQueryableType.Name) != null)
            {
                resultExpression = CallQueryableMethod(MethodName.Where, new[] {sourceType}, source, Expression.Quote(lambdaExpression));
            }
            else if (source.Type.GetInterface(IEnumerableType.Name) != null)
            {
                resultExpression = CallEnumerableMethod(MethodName.Where, new[] {sourceType}, source, lambdaExpression);
            }
            
            if (resultExpression == null)
            {
                throw new SpoltyException("source not implemented any required interfaces");
            }

            return resultExpression;
        }
    }
}