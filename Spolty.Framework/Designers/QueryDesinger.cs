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
    /// <summary>
    /// QueryDesigner class 
    /// </summary>
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

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public Expression Expression
        {
            get { return _expression; }
            protected set { _expression = value; }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public Type ElementType { get; private set; }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public IQueryProvider Provider { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="root"></param>
        public QueryDesinger(object context, JoinNode root)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(root, "root");

            _context = context;
            ElementType = root.EntityType;

            InitializeQueryable();
            AddJoin(root);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType"></param>
        public QueryDesinger(object context, Type entityType)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(entityType, "entityType");

            _context = context;
            ElementType = entityType;

            InitializeQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryable"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns>Current <see cref="QueryDesinger"> that filtered by conditions. </returns>
        public QueryDesinger AddConditions(params BaseCondition[] conditions)
        {
            return AddConditions(conditions as IEnumerable<BaseCondition>);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns>Current <see cref="QueryDesinger"> that filtered by conditions. </returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderings"></param>
        /// <returns>Current <see cref="QueryDesinger"> that contains orderings.</returns>
        public QueryDesinger AddOrderings(params Ordering[] orderings)
        {
            return AddOrderings(orderings as IEnumerable<Ordering>);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderings"></param>
        /// <returns>Current <see cref="QueryDesinger"> that contains orderings.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptQueryable"></param>
        /// <returns></returns>
        public QueryDesinger Except(IQueryable exceptQueryable)
        {
            Checker.CheckArgumentNull(exceptQueryable, "exceptQueryable");

            _expression = _expressionMakerFactory.CreateExceptExpressionMaker().Make(_expression,
                                                                                    exceptQueryable.Expression);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unionQueryable"></param>
        /// <returns></returns>
        public QueryDesinger Union(IQueryable unionQueryable)
        {
            Checker.CheckArgumentNull(unionQueryable, "unionQueryable");

            _expression = _expressionMakerFactory.CreateUnionExpressionMaker().Make(_expression,
                                                                                   unionQueryable.Expression);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="parameterses"></param>
        /// <returns></returns>
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

        #region Skip, Take, Distinct

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements. 
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements. </param>
        /// <returns>Current <see cref="QueryDesinger"> that contains elements that occur after the specified index in the input sequence. </returns>
        public QueryDesinger Skip(int count)
        {
            if (count > 0)
            {
                _expression = _expressionMakerFactory.CreateSkipExpressionMaker().Make(count, _expression);
            }

            return this;
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return. </param>
        /// <returns>Current <see cref="QueryDesinger"> that contains the specified number of elements from the start of source. </returns>
        public QueryDesinger Take(int count)
        {
            if (count > 0)
            {
                _expression = _expressionMakerFactory.CreateTakeExpressionMaker().Make(count, _expression);
            }

            return this;
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values. 
        /// </summary>
        /// <returns>Returns distinct elements from a sequence by using the default equality comparer to compare values.</returns>
        public QueryDesinger Distinct()
        {
            _expression = _expressionMakerFactory.CreateDistinctExpressionMaker().Make(_expression);

            return this;
        }

        #endregion

        #region Private methods

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

        #endregion
   }
}