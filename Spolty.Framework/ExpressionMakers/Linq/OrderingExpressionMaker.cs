using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parameters.Orderings.Enums;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class OrderingExpressionMaker : ExpressionMaker, IOrderingExpressionMaker
    {
        public OrderingExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        #region IOrderingExpressionMaker Members

        public Expression Make(Expression source, IEnumerable<Ordering> orderings)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(orderings, "orderings");
            var orderingList = new List<Ordering>(orderings);

            if (orderings != null && orderingList.Count > 0)
            {
                Ordering firstOrdering = orderingList[0];
                string[] properties = firstOrdering.ColumnName.Split('.');
                Type elementType = GetTemplateType(source);

                if (firstOrdering.ElementType == null || firstOrdering.ElementType == elementType)
                {
                    PropertyInfo property = elementType.GetProperty(properties[0]);

                    if (property != null &&
                        (!property.PropertyType.IsGenericType ||
                         property.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>)))
                    {
                        string sourceName = elementType.Name;
                        ParameterExpression expressionOrder = GetParameterExpression(elementType, sourceName);

                        LambdaExpression orderingExpression = GetLambdaExpression(elementType, firstOrdering.ColumnName,
                                                                                  expressionOrder, null);
                        var typeArguments = new Type[2];
                        typeArguments[0] = elementType;
                        typeArguments[1] = orderingExpression.Body.Type;

                        source = firstOrdering.SortDirection == SortDirection.Ascending
                                     ? CallQueryableMethod("OrderBy", typeArguments, source, orderingExpression)
                                     : CallQueryableMethod("OrderByDescending", typeArguments, source,
                                                           orderingExpression);

                        if (orderingList.Count > 1)
                        {
                            for (int i = 1; i < orderingList.Count; i++)
                            {
                                orderingExpression = GetLambdaExpression(elementType, orderingList[i].ColumnName,
                                                                         expressionOrder, null);
                                typeArguments[1] = orderingExpression.Body.Type;

                                source = orderingList[i].SortDirection == SortDirection.Ascending
                                             ? CallQueryableMethod("ThenBy", typeArguments, source, orderingExpression)
                                             : CallQueryableMethod("ThenByDescending", typeArguments, source,
                                                                   orderingExpression);
                            }
                        }
                    }
                }
            }
            return source;
        }

        #endregion
    }
}