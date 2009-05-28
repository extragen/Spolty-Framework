using System.Collections.Generic;
using System.Linq.Expressions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Orderings;

namespace Spolty.Framework.ExpressionMakers
{
    public interface IExpressionMaker
    {
        IExpressionMakerFactory Factory { get; }
    }

    public interface IConditionExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression sourceExpression, IEnumerable<BaseCondition> conditions);

        Expression MakeAggregate(Expression sourceExpression,
                                 ParameterExpression parameter,
                                 ConditionList conditionals,
                                 bool singleItem);
    }

    public interface IOrderingExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression sourceExpression, IEnumerable<Ordering> orderings);
    }

    public interface IJoinExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression outerExpression, Expression innerExpression, JoinNode childNode,
                        ConditionList parameter);
    }
}