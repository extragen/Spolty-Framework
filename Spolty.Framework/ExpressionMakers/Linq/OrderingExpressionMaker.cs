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

        public Expression Make(IEnumerable<Ordering> orderings, Expression source)
        {
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(orderings, "orderings");
            var orderingList = new List<Ordering>(orderings);

            if (orderings != null && orderingList.Count > 0)
            {
                for (int i = 0; i < orderingList.Count; i++)
                {
                    Ordering ordering = orderingList[i];
                    string[] properties = ordering.ColumnName.Split('.');
                    Type sourceType = GetTemplateType(source);
                    Type elementType = ordering.ElementType ?? sourceType;

                    PropertyInfo property;
                    ParameterExpression sourceParameter = GetParameterExpression(sourceType, sourceType.Name);
                    MemberExpression propertyExpression = null;
                    if (elementType != sourceType)
                    {
                        var relationProperty = sourceType.GetProperty(elementType.Name);
                        propertyExpression = Expression.Property(sourceParameter, relationProperty);
                        property = elementType.GetProperty(ordering.ColumnName);
//                        propertyExpression = Expression.Property(relationPropertyExpression, property);
                    }
                    else
                    {
                        property = elementType.GetProperty(properties[0]);
                    }

                    if (property != null &&
                        (!property.PropertyType.IsGenericType ||
                         property.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>)))
                    {

                        LambdaExpression orderingExpression = GetLambdaExpression(elementType,
                                                                                  ordering.ColumnName,
                                                                                  sourceParameter, propertyExpression);
                        var typeArguments = new Type[2];
                        typeArguments[0] = sourceType;
                        typeArguments[1] = orderingExpression.Body.Type;

                        source = ordering.SortDirection == SortDirection.Ascending
                                     ? CallQueryableMethod("OrderBy", typeArguments, source, orderingExpression)
                                     : CallQueryableMethod("OrderByDescending", typeArguments, source,
                                                           orderingExpression);
                    }
                }
            }
            return source;
        }

        #endregion

    }
}