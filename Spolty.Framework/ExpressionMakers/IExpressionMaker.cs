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
        Expression Make(IEnumerable<BaseCondition> conditions, Expression sourceExpression);

        Expression MakeAggregate(Expression sourceExpression,
                                 ParameterExpression parameter,
                                 ConditionList conditionals,
                                 bool singleItem);
    }

    public interface IOrderingExpressionMaker : IExpressionMaker
    {
        Expression Make(IEnumerable<Ordering> orderings, Expression sourceExpression);
    }

    public interface IJoinExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression outerExpression, Expression innerExpression, JoinNode childNode,
                        ConditionList parameter);
    }

    public interface ISkipExpressionMaker : IExpressionMaker
    {
        Expression Make(int skip, Expression source);
    }

    public interface ITakeExpressionMaker : IExpressionMaker
    {
        Expression Make(int take, Expression source);
    }

    public interface IExceptExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression source, Expression except);
    }

    public interface IUnionExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression source, Expression union);
    }    
    
    public interface IDistinctExpressionMaker : IExpressionMaker
    {
        Expression Make(Expression source);
    }
}