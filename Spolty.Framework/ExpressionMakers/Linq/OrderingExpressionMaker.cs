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
                for (int i = 0; i < orderingList.Count; i++)
                {
                    Type sourceType = GetTemplateType(source);
                    Ordering ordering = orderingList[i];
                    string propertyName = ordering.ColumnName;
                    Type elementType = ordering.ElementType ?? sourceType;

                    PropertyInfo property;
                    ParameterExpression sourceParameter = GetParameterExpression(sourceType, sourceType.Name);
                    MemberExpression propertyExpression = null;
                    if (elementType != sourceType)
                    {
                        var relationProperty = sourceType.GetProperty(elementType.Name);
                        
                        if (relationProperty == null)
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
                         property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {

                        LambdaExpression orderingExpression = GetLambdaExpression(elementType,
                                                                                  ordering.ColumnName,
                                                                                  sourceParameter, propertyExpression);
                        var typeArguments = new Type[2];
                        typeArguments[0] = sourceType;
                        typeArguments[1] = orderingExpression.Body.Type;

                        source = ordering.SortDirection == SortDirection.Ascending
                                     ? CallQueryableMethod(MethodName.OrderBy, typeArguments, source, orderingExpression)
                                     : CallQueryableMethod(MethodName.OrderByDescending, typeArguments, source,
                                                           orderingExpression);
                    }
                }
            }
            return source;
        }

        #endregion

    }
}