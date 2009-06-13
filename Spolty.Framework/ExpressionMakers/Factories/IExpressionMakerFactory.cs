using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Spolty.Framework.EnumeratorProviders;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public interface IExpressionMakerFactory
    {
        object CurrentContext { get; }
        Dictionary<string, object> Store { get; }  
        ISimpleExpressionMaker CreateSimpleExpressionMaker();
        IJoinExpressionMaker CreateJoinExpressionMaker();
        IOrderingExpressionMaker CreateOrderingExpressionMaker();
        IConditionExpressionMaker CreateConditionExpressionMaker();
        IQueryable GetTable(Type entityType);
        IEnumeratorProvider CreateEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression);
    }
}