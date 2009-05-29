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
        IQueryable GetTable(Type entityType);
    }
}