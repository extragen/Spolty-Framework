using System;
using System.Data.Objects;
using System.Linq;
using Spolty.Framework.Checkers;
using Spolty.Framework.ExpressionMakers.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public class EntityFrameworkExpressionMakerFactory : IExpressionMakerFactory
    {
        public EntityFrameworkExpressionMakerFactory(object currentContext)
        {
            Checker.CheckArgumentNull(currentContext as ObjectContext, "currentContext");
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

        public IQueryable GetTable(Type entityType)
        {
            var context = ((ObjectContext) CurrentContext);
            Type oq = typeof (ObjectQuery<>).MakeGenericType(entityType);
            var objectQuery =
                (ObjectQuery)
                oq.GetConstructors()[0].Invoke(new object[] {String.Format("[{0}]", entityType.Name), context});
            return objectQuery;
        }

        #endregion
    }
}