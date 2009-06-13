using System.Collections.Generic;
using System.Linq.Expressions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Orderings;

namespace Spolty.Framework.ExpressionMakers
{
    public interface IExpressionMaker
    {
        IExpressionMakerFactory Factory { get; }
        Expression MakeDistinct(Expression sourceExpression);
        Expression MakeCount(Expression sourceExpression, Expression conditionExpression);
        Expression MakeAny(Expression sourceExpression, Expression conditionExpression);
        Expression MakeAll(Expression sourceExpression, Expression conditionExpression);
        Expression MakeFirst(Expression sourceExpression);
        Expression MakeFirstOrDefault(Expression sourceExpression);
        Expression MakeUnion(Expression sourceExpression, Expression unionExpression);
        Expression MakeExcept(Expression sourceExpression, Expression exceptExpression);
        Expression MakeTake(int count, Expression sourceExpression);
        Expression MakeSkip(int count, Expression sourceExpression);
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
        Expression Make(Expression rootExpression, JoinNode rootNode, params IParameterMarker[] parameters);
    }
}