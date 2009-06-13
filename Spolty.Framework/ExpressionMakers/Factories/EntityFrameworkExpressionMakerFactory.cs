using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using Spolty.Framework.Checkers;
using Spolty.Framework.Designers;
using Spolty.Framework.ExpressionMakers.EntityFramework;
using Spolty.Framework.ExpressionMakers.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    public class EntityFrameworkExpressionMakerFactory : IExpressionMakerFactory
    {
        public EntityFrameworkExpressionMakerFactory(object currentContext)
        {
            Checker.CheckArgumentNull(currentContext as ObjectContext, "currentContext");
            CurrentContext = currentContext;
            Store = new Dictionary<string, object>(0);
        }

        #region IExpressionMakerFactory Members

        #region Implementation of IExpressionMakerFactory

        public Dictionary<string, object> Store { get; private set; }

        #endregion

        public object CurrentContext { get; private set; }

        public IJoinExpressionMaker CreateJoinExpressionMaker()
        {
            return new EFJoinMaker(this);
        }

        public IOrderingExpressionMaker CreateOrderingExpressionMaker()
        {
            return new OrderingExpressionMaker(this);
        }

        public IConditionExpressionMaker CreateConditionExpressionMaker()
        {
            return new ConditionExpressionMaker(this);
        }

        public IExpressionMaker CreateExpressionMaker()
        {
            return new ExpressionMaker(this);
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

        public IEnumeratorProvider CreateEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression)
        {
            return new DefaultEnumeratorProvider(entityType, provider, expression);
        }

        #endregion
    }
}