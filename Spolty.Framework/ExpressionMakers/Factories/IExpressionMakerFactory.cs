using System;
using System.Collections.Generic;
using System.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public interface IExpressionMakerFactory
    {
        object CurrentContext { get; }
        Dictionary<string, object> Store { get; }  
        IExpressionMaker CreateExpressionMaker();
        IJoinExpressionMaker CreateJoinExpressionMaker();
        IOrderingExpressionMaker CreateOrderingExpressionMaker();
        IConditionExpressionMaker CreateConditionExpressionMaker();
        IQueryable GetTable(Type entityType);
    }
}