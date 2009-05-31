using Spolty.Framework.ExpressionMakers.Factories;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class AggregateExpressionMaker : ExpressionMaker
    {
        public AggregateExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

//        public static List<Expression> MakeAggregateMethodExpression(Expression sourceExpression, JoinNode node,
//                                                                     ParameterExpression parameter)
//        {
//            List<Expression> expressions = new List<Expression>();
//            foreach (AggregateMethod aggregateMethod in node.ConditionAggregateMethod)
//            {
//                expressions.Add(
//                    MakeAggregateMethodExpression(sourceExpression, node.EntityType, aggregateMethod, parameter));
//            }
//            return expressions;
//        }
//
//        public static Expression MakeAggregateMethodExpression(Expression sourceExpression, Type innerSourceType,
//                                                               AggregateMethod simpleAggregateMethod,
//                                                               ParameterExpression parameter)
//        {
//            Expression resultExpression = sourceExpression;
//            if (simpleAggregateMethod.AggregateMethodType == AggregateMethodType.None)
//            {
//                return null;
//            }
//
//            Type sourceType = GetTemplateType(sourceExpression);
//            LambdaExpression lambdaExpression;
//            PropertyInfo propertyInfo = null;
//
//            if (simpleAggregateMethod is ComplicatedAggregateMethod)
//            {
//                propertyInfo = ((ComplicatedAggregateMethod) simpleAggregateMethod).PropertyInfo;
//            }
//
//            if (innerSourceType != null && sourceType != innerSourceType)
//            {
//                ParameterExpression innerParameter = GetParameterExpression(innerSourceType, innerSourceType.Name);
//                FieldInfo fieldInfo = sourceType.GetField(innerSourceType.Name);
//                if (fieldInfo == null)
//                {
//                    throw new SpoltyException(String.Format("Field {0} is not found", innerSourceType.Name));
//                }
//                MemberExpression memberExpression = Expression.Field(parameter, fieldInfo);
//
//                if (simpleAggregateMethod.AggregateMethodType != AggregateMethodType.Count &&
//                    simpleAggregateMethod.AggregateMethodType != AggregateMethodType.LongCount)
//                {
//                    if (simpleAggregateMethod.AggregateMethodType == AggregateMethodType.Aggregate)
//                    {
//                        //TODO:Aggregate
//                        //					lambdaExpression = QueryExpression.Lambda(String.Concat(innerType.Name, ".", node.ConditionAggregateMethod.PropertyInfo.Name), innerParameter);
//                        //					resultExpression = ....
//                    }
//                    else
//                    {
//                        if (propertyInfo != null)
//                        {
//                            lambdaExpression =
//                                QueryExpression.Lambda(String.Concat(innerSourceType.Name, ".", propertyInfo.Name),
//                                                       innerParameter);
//                            resultExpression =
//                                CallMethodExpression(SequenceMethodList,
//                                                     simpleAggregateMethod.AggregateMethodType.ToString(), 2,
//                                                     propertyInfo.PropertyType, new Type[] {innerSourceType},
//                                                     memberExpression, lambdaExpression);
//                        }
//                        else
//                        {
//                            throw new SpoltyException("propertyInfo is not defined");
//                        }
//                    }
//                }
//                else
//                {
//                    resultExpression =
//                        CallMethodExpression(SequenceMethodList, simpleAggregateMethod.AggregateMethodType.ToString(), 1,
//                                             simpleAggregateMethod.AggregateMethodType == AggregateMethodType.Count
//                                                 ? typeof (int)
//                                                 : typeof (long),
//                                             new Type[] {innerSourceType}, memberExpression);
//                }
//            }
//            else
//            {
//                if (propertyInfo != null)
//                {
//                    lambdaExpression =
//                        QueryExpression.Lambda(String.Concat(sourceType.Name, ".", propertyInfo.Name), parameter);
//
//                    switch (simpleAggregateMethod.AggregateMethodType)
//                    {
//                        case AggregateMethodType.Aggregate:
//                            //						resultExpression = QueryExpression.Aggregate();
//                            break;
//                        case AggregateMethodType.Average:
//                            resultExpression = QueryExpression.Average(sourceExpression, lambdaExpression);
//                            break;
//                        case AggregateMethodType.Max:
//                            resultExpression = QueryExpression.Max(sourceExpression, lambdaExpression);
//                            break;
//                        case AggregateMethodType.Min:
//                            resultExpression = QueryExpression.Min(sourceExpression, lambdaExpression);
//                            break;
//                        case AggregateMethodType.Sum:
//                            resultExpression = QueryExpression.Sum(sourceExpression, lambdaExpression);
//                            break;
//                    }
//                }
//                else
//                {
//                    switch (simpleAggregateMethod.AggregateMethodType)
//                    {
//                        case AggregateMethodType.Count:
//                            resultExpression = QueryExpression.Count(sourceExpression);
//                            break;
//                        case AggregateMethodType.LongCount:
//                            resultExpression = QueryExpression.LongCount(sourceExpression);
//                            break;
//                        default:
//                            throw new NotSupportedException(simpleAggregateMethod.AggregateMethodType.ToString());
//                    }
//                }
//            }
//
//            return resultExpression;
//        }
    }
}