using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using Spolty.Framework.Activators;
using Spolty.Framework.Checkers;
using Spolty.Framework.ConfigurationSections;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.ExpressionMakers.Linq;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Loads;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parsers;

namespace Spolty.Framework.Designers
{
    public class QueryDesinger : IQueryable, IEnumerable
    {
        #region Private Fields

        private readonly object _context;
        private IExpressionMakerFactory _expressionMakerFactory;
        private Expression _expression;
        private List<LambdaExpression> _loadLambdaExpressions;

        #endregion

        #region IQueryable Members

        public IEnumerator GetEnumerator()
        {
            DataLoadOptions options = null;

            if (_loadLambdaExpressions != null && _loadLambdaExpressions.Count > 0)
            {
                options = new DataLoadOptions();
                foreach (LambdaExpression lambdaExpression in _loadLambdaExpressions)
                {
                    options.LoadWith(lambdaExpression);
                }
            }

//            _context.LoadOptions = options;
            object returnValue = Provider.Execute(_expression);
//            _context.LoadOptions = null;

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
            ElementType = root.BizObjectType;

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

        public QueryDesinger AddConditions(ConditionList conditionals)
        {
            if (conditionals == null || conditionals.Count == 0)
            {
                return this;
            }

            conditionals.RemoveDuplicates();

            _expression = _expressionMakerFactory.CreateConditionExpressionMaker().Make(_expression, conditionals);

            return this;
        }

        public QueryDesinger AddOrderings(OrderingList orderings)
        {
            if (orderings == null || orderings.Count == 0)
            {
                return this;
            }

            orderings.RemoveDuplicates();

            _expression = _expressionMakerFactory.CreateOrderingExpressionMaker().Make(_expression, orderings);

            return this;
        }

        public QueryDesinger Except(IQueryable exceptQueryable)
        {
            Checker.CheckArgumentNull(exceptQueryable, "exceptQueryable");

            _expression = ExpressionMaker.MakeExcept(_expression, exceptQueryable.Expression);

            return this;
        }

        public QueryDesinger Union(IQueryable unionQueryable)
        {
            Checker.CheckArgumentNull(unionQueryable, "unionQueryable");

            _expression = ExpressionMaker.MakeUnion(_expression, unionQueryable.Expression);

            return this;
        }

        public QueryDesinger AddJoin(JoinNode rootNode, params IParameterMarker[] parameterses)
        {
            Checker.CheckArgumentNull(rootNode, "rootNode");

            if (rootNode.BizObjectType != ElementType)
            {
                throw new SpoltyException("BizObjectType of root node mismatch ElementType");
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

        private Expression AddChildren(Expression rootExpression, JoinNode node,
                                       ConditionList conditions, OrderingList orderings)
        {
            Expression newExpression = rootExpression;
            IJoinExpressionMaker maker = _expressionMakerFactory.CreateJoinExpressionMaker();
            foreach (JoinNode childNode in node.ChildNodes)
            {
                if (childNode.ChildNodes.Count > 0)
                {
                    var queryDesinger = new QueryDesinger(_context, childNode.BizObjectType);
                    queryDesinger.AddConditions(conditions);
                    queryDesinger.AddOrderings(orderings);

                    Expression outerExpression = queryDesinger.Expression;
                    Expression childrenExpression = AddChildren(outerExpression, childNode, conditions, orderings);
                    newExpression = maker.Make(newExpression, childrenExpression, childNode,
                                               conditions);
                }
                else
                {
                    var queryDesinger = new QueryDesinger(_context, childNode.BizObjectType);
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

        public void SkipAndTake(int skip, int take)
        {
            Skip(skip);
            Take(take);
        }

        public void Skip(int skip)
        {
            _expression = ExpressionMaker.MakeSkip(_expression, skip);
        }

        public void Take(int take)
        {
            _expression = ExpressionMaker.MakeTake(_expression, take);
        }

        #endregion

        #region Including

        public QueryDesinger LoadTypes(params Type[] includingTypes)
        {
            if (includingTypes == null || includingTypes.Length == 0)
            {
                return this;
            }

            _loadLambdaExpressions = IncludingExpressionMaker.MakeIncluding(_expression, includingTypes);

            return this;
        }

        public QueryDesinger LoadTypes(LoadTree loadTree)
        {
            if (loadTree == null || loadTree.Root == null)
            {
                return this;
            }
            _loadLambdaExpressions = IncludingExpressionMaker.MakeIncluding(ElementType, _expression, loadTree.Root);

            return this;
        }

        #endregion
    }
}