using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Spolty.Framework.Activators;
using Spolty.Framework.Checkers;
using Spolty.Framework.ConfigurationSections;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.BaseNode;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parsers;

namespace Spolty.Framework.Designers
{
    public class QueryDesinger : IQueryable, IEnumerable
    {
        #region Private Fields

        private readonly object _context;
        private Expression _expression;
        private IExpressionMakerFactory _expressionMakerFactory;

        #endregion

        #region IQueryable Members

        public IEnumerator GetEnumerator()
        {
            object returnValue = Provider.Execute(_expression);

            return ((IEnumerable) returnValue).GetEnumerator();
        }

        public Expression Expression
        {
            get { return _expression; }
            protected set { _expression = value; }
        }

        public Type ElementType { get; private set; }

        public IQueryProvider Provider { get; private set; }

        #endregion

        #region Constructors

        public QueryDesinger(object context, JoinNode root)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(root, "root");

            _context = context;
            ElementType = root.EntityType;

            InitializeQueryable();
            AddJoin(root);
        }

        public QueryDesinger(object context, Type entityType)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(entityType, "entityType");

            _context = context;
            ElementType = entityType;

            InitializeQueryable();
        }

        public QueryDesinger(object context, IQueryable queryable)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(queryable, "queryable");

            _context = context;
            ElementType = queryable.ElementType;

            _expressionMakerFactory = CreateExpressionMakerFactory();
            Provider = queryable.Provider;
            _expression = queryable.Expression;
        }

        #endregion

        #region Conditons methods

        public QueryDesinger AddConditions(params BaseCondition[] conditions)
        {
            return AddConditions(conditions as IEnumerable<BaseCondition>);
        }

        public QueryDesinger AddConditions(IEnumerable<BaseCondition> conditions)
        {
            if (conditions == null)
            {
                return this;
            }

            var conditionList = new ConditionList(conditions);
            if (conditionList.Count == 0)
            {
                return this;
            }

            conditionList.RemoveDuplicates();

            _expression = _expressionMakerFactory.CreateConditionExpressionMaker().Make(conditions, _expression);

            return this;
        }

        #endregion

        #region Orderings methods

        public QueryDesinger AddOrderings(params Ordering[] orderings)
        {
            return AddOrderings(orderings as IEnumerable<Ordering>);
        }

        public QueryDesinger AddOrderings(IEnumerable<Ordering> orderings)
        {
            if (orderings == null)
            {
                return this;
            }

            var orderingList = new OrderingList(orderings);
            if (orderingList.Count == 0)
            {
                return this;
            }

            orderingList.RemoveDuplicates();

            _expression = _expressionMakerFactory.CreateOrderingExpressionMaker().Make(orderings, _expression);

            return this;
        }

        #endregion

        public QueryDesinger Except(IQueryable exceptQueryable)
        {
            Checker.CheckArgumentNull(exceptQueryable, "exceptQueryable");

            _expression = _expressionMakerFactory.CreateExceptExpressionMaker().Make(_expression,
                                                                                    exceptQueryable.Expression);

            return this;
        }

        public QueryDesinger Union(IQueryable unionQueryable)
        {
            Checker.CheckArgumentNull(unionQueryable, "unionQueryable");

            _expression = _expressionMakerFactory.CreateUnionExpressionMaker().Make(_expression,
                                                                                   unionQueryable.Expression);

            return this;
        }

        public QueryDesinger AddJoin(JoinNode rootNode, params IParameterMarker[] parameterses)
        {
            Checker.CheckArgumentNull(rootNode, "rootNode");

            if (rootNode.EntityType != ElementType)
            {
                throw new SpoltyException("EntityType of root node mismatch ElementType");
            }

            OrderingList orderings;
            ConditionList conditions;

            ParametersParser.Parse(out conditions, out orderings);

            AddConditions(rootNode.Conditions);

            _expression = AddChildren(_expression, rootNode, conditions, orderings);

            AddConditions(conditions);
            AddOrderings(orderings);

            return this;
        }

        private Expression AddChildren(Expression rootExpression, BaseNode node,
                                       ConditionList conditions, IEnumerable<Ordering> orderings)
        {
            Expression newExpression = rootExpression;
            IJoinExpressionMaker maker = _expressionMakerFactory.CreateJoinExpressionMaker();
            foreach (JoinNode childNode in node.ChildNodes)
            {
                if (childNode.ChildNodes.Count > 0)
                {
                    var queryDesinger = new QueryDesinger(_context, childNode.EntityType);
                    queryDesinger.AddConditions(conditions);
                    queryDesinger.AddOrderings(orderings);

                    Expression outerExpression = queryDesinger.Expression;
                    Expression childrenExpression = AddChildren(outerExpression, childNode, conditions, orderings);
                    newExpression = maker.Make(newExpression, childrenExpression, childNode,
                                               conditions);
                }
                else
                {
                    var queryDesinger = new QueryDesinger(_context, childNode.EntityType);
                    queryDesinger.AddConditions(childNode.Conditions);
                    queryDesinger.AddOrderings(orderings);

                    newExpression = maker.Make(newExpression, queryDesinger.Expression, childNode,
                                               conditions);
                }
            }
            return newExpression;
        }

        private void InitializeQueryable()
        {
            _expressionMakerFactory = CreateExpressionMakerFactory();
            IQueryable queryable = _expressionMakerFactory.GetTable(ElementType);

            if (queryable == null)
            {
                throw new SpoltyException(String.Format("IQueryable not found for type: {0}", ElementType.FullName));
            }

            Provider = queryable.Provider;
            _expression = queryable.Expression;
        }

        private IExpressionMakerFactory CreateExpressionMakerFactory()
        {
            FactoryConfigurationCollection collection = SpoltyFrameworkSectionHandler.Instance.Factories;
            return
                SpoltyActivator.CreateInstance<IExpressionMakerFactory>(
                    collection.UseFactory.Type, new[] {_context});
        }

        #region Skip And Take

        public QueryDesinger SkipAndTake(int skip, int take)
        {
            Skip(skip);
            Take(take);

            return this;
        }

        public QueryDesinger Skip(int skip)
        {
            if (skip > 0)
            {
                _expression = _expressionMakerFactory.CreateSkipExpressionMaker().Make(skip, _expression);
            }

            return this;
        }

        public QueryDesinger Take(int take)
        {
            if (take > 0)
            {
                _expression = _expressionMakerFactory.CreateTakeExpressionMaker().Make(take, _expression);
            }

            return this;
        }

        public QueryDesinger Distinct()
        {
            _expression = _expressionMakerFactory.CreateDistinctExpressionMaker().Make(_expression);

            return this;
        }

        #endregion
    }
}