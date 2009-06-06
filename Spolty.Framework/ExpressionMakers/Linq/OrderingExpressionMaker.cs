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
        public OrderingExpressionMaker(IExpressionMakerFactory factory)
            : base(factory)
        {
        }

        #region IOrderingExpressionMaker Members

        public Expression Make(IEnumerable<Ordering> orderings, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(orderings, "orderings");
            var orderingList = new List<Ordering>(orderings);

            if (orderings != null && orderingList.Count > 0)
            {
                var alredyOrdered = new List<Type>();
                for (int i = 0; i < orderingList.Count; i++)
                {
                    Type sourceType = GetGenericType(source);
                    Ordering ordering = orderingList[i];
                    string propertyName = ordering.ColumnName;
                    Type elementType = ordering.ElementType ?? sourceType;

                    PropertyInfo property;
                    ParameterExpression sourceParameter = CreateOrGetParameterExpression(sourceType, sourceType.Name);
                    MemberExpression propertyExpression = null;
                    if (elementType != sourceType)
                    {
                        PropertyInfo relationProperty = sourceType.GetProperty(elementType.Name);

                        if (relationProperty == null || relationProperty.PropertyType.IsGenericType)
                        {
                            continue;
                        }

                        propertyExpression = Expression.Property(sourceParameter, relationProperty);
                        property = elementType.GetProperty(ordering.ColumnName);
                    }
                    else
                    {
                        property = elementType.GetProperty(propertyName);
                    }

                    if (property != null &&
                        (!property.PropertyType.IsGenericType ||
                         property.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>)))
                    {
                        LambdaExpression orderingExpression = CreateLambdaExpression(elementType,
                                                                                  ordering.ColumnName,
                                                                                  sourceParameter, propertyExpression);
                        var typeArguments = new Type[2];
                        typeArguments[0] = sourceType;
                        typeArguments[1] = orderingExpression.Body.Type;

                        string orderByMethod = MethodName.OrderBy;
                        string orderByDescendingMethod = MethodName.OrderByDescending;
                        
                        if (alredyOrdered.Contains(sourceType))
                        {
                            orderByMethod = MethodName.ThenBy;
                            orderByDescendingMethod = MethodName.ThenByDescending;
                        }

                        source = ordering.SortDirection == SortDirection.Ascending
                                     ? CallQueryableMethod(orderByMethod, typeArguments, source, orderingExpression)
                                     : CallQueryableMethod(orderByDescendingMethod, typeArguments, source,
                                                           orderingExpression);
                    }
                }
            }
            return source;
        }

        #endregion
    }
}