﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using Spolty.Framework.Checkers;
using Spolty.Framework.EnumeratorProviders;
using Spolty.Framework.ExpressionMakers.Linq;

namespace Spolty.Framework.ExpressionMakers.Factories
{
    /// <summary>
    /// Class defines factory.
    /// </summary>
    public class LinqExpressionMakerFactory : IExpressionMakerFactory
    {
        public LinqExpressionMakerFactory(object currentContext)
        {
            Checker.CheckArgumentNull(currentContext as DataContext, "currentContext");
            CurrentContext = currentContext;
            Store = new Dictionary<string, object>(1);
        }

        #region IExpressionMakerFactory Members

        #region Implementation of IExpressionMakerFactory

        public Dictionary<string, object> Store { get; private set; }

        #endregion

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

        public ISimpleExpressionMaker CreateSimpleExpressionMaker()
        {
            return new ExpressionMaker(this);
        }

        public IQueryable GetTable(Type entityType)
        {
            return ((DataContext) CurrentContext).GetTable(entityType);
        }

        public IEnumeratorProvider CreateEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression)
        {
            return new LinqEnumeratorProvider(entityType, provider, expression);
        }

        #endregion
    }
}