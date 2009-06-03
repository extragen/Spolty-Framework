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
        Expression MakeDistinct(Expression source);
        Expression MakeCount(Expression source);
        Expression MakeAny(Expression source);
        Expression MakeFirst(Expression source);
        Expression MakeFirstOrDefault(Expression source);
        Expression MakeUnion(Expression source, Expression union);
        Expression MakeExcept(Expression source, Expression except);
        Expression MakeTake(int count, Expression source);
        Expression MakeSkip(int count, Expression source);
    }

    public interface IConditionExpressionMaker : IExpressionMaker
    {
        Expression Make(IEnumerable<BaseCondition> conditions, Expression sourceExpression);
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
}