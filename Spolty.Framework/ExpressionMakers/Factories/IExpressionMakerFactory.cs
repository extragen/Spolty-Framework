using System;
using System.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public interface IExpressionMakerFactory
    {
        object CurrentContext { get; }
        IJoinExpressionMaker CreateJoinExpressionMaker();
        IOrderingExpressionMaker CreateOrderingExpressionMaker();
        IConditionExpressionMaker CreateConditionExpressionMaker();
        ISkipExpressionMaker CreateSkipExpressionMaker();
        ITakeExpressionMaker CreateTakeExpressionMaker();
        IExceptExpressionMaker CreateExceptExpressionMaker();
        IUnionExpressionMaker CreateUnionExpressionMaker();
        IDistinctExpressionMaker CreateDistinctExpressionMaker();
        IQueryable GetTable(Type entityType);
    }
}