using System;
using System.Data.Linq;
using System.Linq;
using Spolty.Framework.Checkers;
using Spolty.Framework.ExpressionMakers.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public class LinqExpressionMakerFactory : IExpressionMakerFactory
    {
        public LinqExpressionMakerFactory(object currentContext)
        {
            Checker.CheckArgumentNull(currentContext as DataContext, "currentContext");
            CurrentContext = currentContext;
        }

        #region IExpressionMakerFactory Members

        public object CurrentContext { get; private set; }

        public IJoinExpressionMaker CreateJoinExpressionMaker()
        {
            return new JoinExpressionMaker(this);
        }

        public IOrderingExpressionMaker CreateOrderingExpressionMaker()
        {
            return new OrderingExpressionMaker(this);
        }

        public IConditionExpressionMaker CreateConditionExpressionMaker()
        {
            return new ConditionExpressionMaker(this);
        }

        public ISkipExpressionMaker CreateSkipExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public ITakeExpressionMaker CreateTakeExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public IExceptExpressionMaker CreateExceptExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public IUnionExpressionMaker CreateUnionExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public IDistinctExpressionMaker CreateDistinctExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public IQueryable GetTable(Type entityType)
        {
            return ((DataContext) CurrentContext).GetTable(entityType);
        }

        #endregion
    }
}