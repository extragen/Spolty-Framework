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
using Spolty.Framework.Helpers;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parsers;

namespace Spolty.Framework.Designers
{
    /// <summary>
    /// QueryDesigner class 
    /// </summary>
    public class QueryDesinger : IQueryable, IEnumerable, ICloneable
    {
        #region Private Fields

        private readonly object _context;
        private Expression _expression;
        private IExpressionMakerFactory _expressionMakerFactory;

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new QueryDesinger(_context, this);
        }

        #endregion

        #region IQueryable Members

        public IEnumerator GetEnumerator()
        {
            return _expressionMakerFactory.CreateEnumeratorProvider(ElementType, Provider, _expression).GetEnumerator();
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

        private QueryDesinger(object context, Type elementType, IQueryProvider provider, Expression expression, IExpressionMakerFactory factory)
        {
            _context = context;
            ElementType = elementType;
            Provider = provider;
            Expression = expression;
            _expressionMakerFactory = factory;
        }

        /// <summary>
        /// Creates <see cref="QueryDesinger"/> by <see cref="JoinNode"/> tree. 
        /// </summary>
        /// <param name="context">object context.</param>
        /// <param name="root">root <see cref="JoinNode"/>. Defines result type of <see cref="T:System.Linq.IQueryProvider"/> ElementType</param>
        /// <param name="parameters">additional parameters <see cref="IParameterMarker"/> use for creation 
        /// additional condition <see cref="ConditionList"/> and/or ordering <see cref="OrderingList"/>.</param>
        public QueryDesinger(object context, JoinNode root, params IParameterMarker[] parameters)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(root, "root");

            _context = context;
            ElementType = root.EntityType;

            InitializeQueryable();
            QueryDesinger queryDesinger = Join(root, parameters);
            _expression = queryDesinger.Expression;
        }

        /// <summary>
        /// Creates <see cref="QueryDesinger"/> by type of entity from context.
        /// </summary>
        /// <param name="context">object context.</param>
        /// <param name="entityType">type of entity.</param>
        public QueryDesinger(object context, Type entityType)
        {
            Checker.CheckArgumentNull(context, "context");
            Checker.CheckArgumentNull(entityType, "entityType");

            _context = context;
            ElementType = entityType;

            InitializeQueryable();
        }

        /// <summary>
        /// Creates <see cref="QueryDesinger"/> by <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="context">object context</param>
        /// <param name="queryable">already formed queryable. 
        /// It can be Linq To Sql formed query or formed by <see cref="QueryDesinger"/></param>
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
        /// Adds to current <see cref="QueryDesinger"/> additional conditions.
        /// </summary>
        /// <param name="conditions">additional conditions.</param>
        /// <returns>A <see cref="QueryDesinger"/> that filtered by conditions. </returns>
        public QueryDesinger Where(params BaseCondition[] conditions)
        {
            return Where(conditions as IEnumerable<BaseCondition>);
        }

        /// <summary>
        /// Adds to current <see cref="QueryDesinger"/> additional conditions.
        /// </summary>
        /// <param name="conditions">additional conditions. </param>
        /// <returns>A <see cref="QueryDesinger"/> that filtered by conditions. </returns>
        public QueryDesinger Where(IEnumerable<BaseCondition> conditions)
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

            Expression expression = _expressionMakerFactory.CreateConditionExpressionMaker().Make(conditions, _expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        #endregion

        #region Orderings methods

        /// <summary>
        /// Adds orderings to <see cref="QueryDesinger"/>. Orderings apply only for current EntityType. 
        /// </summary>
        /// <remarks>If you want apply orderings to joined types you should use Join method with defined <see cref="OrderingList"/> parameter.</remarks>
        /// <param name="orderings"></param>
        /// <returns>A <see cref="QueryDesinger"/> that contains orderings.</returns>
        public QueryDesinger OrderBy(params Ordering[] orderings)
        {
            return OrderBy(orderings as IEnumerable<Ordering>);
        }

        /// <summary>
        /// Adds orderings to <see cref="QueryDesinger"/>. Orderings apply only for current EntityType. 
        /// </summary>
        /// <remarks>If you want apply orderings to joined types you should use Join method with defined <see cref="OrderingList"/> parameter.</remarks>
        /// <param name="orderings"></param>
        /// <returns>A <see cref="QueryDesinger"/> that contains orderings.</returns>
        public QueryDesinger OrderBy(IEnumerable<Ordering> orderings)
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

            Expression expression = _expressionMakerFactory.CreateOrderingExpressionMaker().Make(orderings, _expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        #endregion

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer to compare values. 
        /// First sequence it's current <see cref="QueryDesinger"/>, second it's passing parameter <see cref="IQueryable"/> . 
        /// </summary>
        /// <param name="exceptQueryable">An <see cref="IQueryable"/> whose elements that also occur in the first sequence will not appear in the returned sequence. </param>
        /// <returns>A <see cref="QueryDesinger"/> that contains the set difference of the two sequences. </returns>
        public QueryDesinger Except(IQueryable exceptQueryable)
        {
            Checker.CheckArgumentNull(exceptQueryable, "exceptQueryable");

            Expression expression = _expressionMakerFactory.CreateSimpleExpressionMaker().MakeExcept(_expression,
                                                                                     exceptQueryable.Expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        /// <summary>
        /// Produces the set union of two sequences by using the default equality comparer. 
        /// First sequence is current <see cref="QueryDesinger"/>.
        /// </summary>
        /// <param name="unionQueryable">A sequence whose distinct elements form the second set for the union operation. </param>
        /// <returns>A <see cref="QueryDesinger"/> that contains the elements from both input sequences, excluding duplicates. </returns>
        public QueryDesinger Union(IQueryable unionQueryable)
        {
            Checker.CheckArgumentNull(unionQueryable, "unionQueryable");

            _expression = _expressionMakerFactory.CreateSimpleExpressionMaker().MakeUnion(_expression,
                                                                                    unionQueryable.Expression);

            return this;
        }

        /// <summary>
        /// Adds joins to current <see cref="QueryDesinger"/> by using <see cref="JoinNode"/>. 
        /// </summary>
        /// <param name="rootNode">root noode should be equal type to current ElementType</param>
        /// <param name="parameteres">additional parameters <see cref="IParameterMarker"/> use for creation 
        /// additional condition <see cref="ConditionList"/> and/or ordering <see cref="OrderingList"/>. 
        /// Orderings in this case could be assigned to joins nodes.</param>
        /// <returns>A <see cref="QueryDesinger"/> which has joins with other Entity filtered and ordered by passed parameteres.</returns>
        public QueryDesinger Join(JoinNode rootNode, params IParameterMarker[] parameteres)
        {
            Checker.CheckArgumentNull(rootNode, "rootNode");

            if (rootNode.EntityType != ElementType)
            {
                throw new SpoltyException("EntityType of root node mismatch ElementType");
            }

            OrderingList orderings;
            ConditionList conditions;

            ParametersParser.Parse(out conditions, out orderings, parameteres);

            QueryDesinger queryDesinger = Where(rootNode.Conditions);

            IJoinExpressionMaker maker = _expressionMakerFactory.CreateJoinExpressionMaker();
            Expression expression = maker.Make(queryDesinger.Expression, rootNode, conditions, orderings);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory)
                .Where(conditions)
                .OrderBy(orderings
                             .Where(order => order.ElementType == null || order.ElementType == ElementType));
        }

        /// <summary>
        /// Converts the elements of an IQueryable to the specified type. 
        /// </summary>
        /// <remarks>Current method execute query if you made left outer join by using <see cref="QueryDesinger"/>.</remarks>
        /// <typeparam name="TResult">The type to convert the elements of source to. </typeparam>
        /// <returns>An <see cref="IQueryable{T}"/> that contains each element of the source sequence converted to the specified type. </returns>
        public IQueryable<TResult> Cast<TResult>()
        {
            if (ElementType == ReflectionHelper.GetGenericType(_expression.Type))
            {
                return Provider.CreateQuery(_expression).Cast<TResult>();
            }

            var results = new List<TResult>();
            IEnumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                results.Add((TResult) enumerator.Current);
            }
            return results.AsQueryable();
        }

        public override string ToString()
        {
            return _expression.ToString();
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements. 
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements. </param>
        /// <returns>A <see cref="QueryDesinger"/> that contains elements that occur after the specified index in the input sequence. </returns>
        public QueryDesinger Skip(int count)
        {
            Expression expression = _expressionMakerFactory.CreateSimpleExpressionMaker().MakeSkip(count, _expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return. </param>
        /// <returns>A <see cref="QueryDesinger"/> that contains the specified number of elements from the start of source. </returns>
        public QueryDesinger Take(int count)
        {
            Expression expression = _expressionMakerFactory.CreateSimpleExpressionMaker().MakeTake(count, _expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values. 
        /// </summary>
        /// <returns>Returns distinct elements from a sequence by using the default equality comparer to compare values.</returns>
        public QueryDesinger Distinct()
        {
            Expression expression = _expressionMakerFactory.CreateSimpleExpressionMaker().MakeDistinct(_expression);

            return new QueryDesinger(_context, ElementType, Provider, expression, _expressionMakerFactory);
        }

        /// <summary>
        /// Returns the number of elements in a sequence. 
        /// </summary>
        /// <returns>The number of elements in the input sequence. </returns>
        public int Count()
        {
            var copy = (QueryDesinger) Clone();

            copy._expression = copy._expressionMakerFactory.CreateSimpleExpressionMaker().MakeCount(copy._expression, null);

            return (int) copy.Provider.Execute(copy._expression);
        }

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <returns>true if the source sequence contains any elements; otherwise, false. </returns>
        public bool Any()
        {
            var copy = (QueryDesinger) Clone();

            copy._expression = copy._expressionMakerFactory.CreateSimpleExpressionMaker().MakeAny(copy._expression, null);

            return (bool) copy.Provider.Execute(copy._expression);
        }

        /// <summary>
        /// Returns the first element of a sequence. 
        /// </summary>
        /// <returns>The first element in current <see cref="QueryDesinger"/>. </returns>
        public object First()
        {
            var copy = (QueryDesinger) Clone();

            copy._expression = copy._expressionMakerFactory.CreateSimpleExpressionMaker().MakeFirst(copy._expression);

            return copy.Provider.Execute(copy._expression);
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements. 
        /// </summary>
        /// <returns>default(ElementType) if source is empty; otherwise, the first element in source. </returns>
        public object FirstOrDefault()
        {
            var copy = (QueryDesinger) Clone();

            copy._expression = copy._expressionMakerFactory.CreateSimpleExpressionMaker().MakeFirstOrDefault(copy._expression);

            return copy.Provider.Execute(copy._expression);
        }

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

        #endregion
    }
}