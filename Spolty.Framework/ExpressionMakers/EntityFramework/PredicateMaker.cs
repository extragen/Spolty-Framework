using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Text;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.ExpressionMakers.EntityFramework
{
    public class PredicateMaker : IConditionExpressionMaker
    {
        private const string DefaultTableName = "it";
        private readonly List<ObjectParameter> _objectParameters;
        private static uint _count;

        public PredicateMaker(IEnumerable<BaseCondition> conditionals) : this(conditionals, DefaultTableName)
        {
        }

        public PredicateMaker(IEnumerable<BaseCondition> conditionals, string tableName)
        {
            if (conditionals == null)
            {
                throw new ArgumentNullException("conditionals");
            }

            if (String.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            TableName = tableName;
            _objectParameters = new List<ObjectParameter>();

            var sb = new StringBuilder();
            foreach (BaseCondition condItem in conditionals)
            {
                string body = MakeSingleCondition(condItem);

                if (body == String.Empty)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(PredicateConstants.AND);
                }
                sb.Append(body);
            }

            Predicate = sb.ToString();
        }

        public ReadOnlyCollection<ObjectParameter> ObjectParameters
        {
            get { return _objectParameters.AsReadOnly(); }
        }

        public string Predicate { get; private set; }
        public string TableName { get; private set; }

        private string MakeSingleCondition(BaseCondition condItem)
        {
            string body = String.Empty;
            if (condItem is Condition)
            {
                var condition = (Condition) condItem;
                ObjectParameter param;
                body = MakeComarisonExpression(condition, out param);
                _objectParameters.Add(param);
            }
            else if (condItem is BiCondition || condItem.GetType().IsSubclassOf(typeof (BiCondition)))
            {
                var biCond = (BiCondition) condItem;

                string leftExpr = null;
                string rightExpr = null;

                if (biCond.LeftCondition == null && biCond.RightCondition == null)
                {
                    return null;
                }

                if (biCond.LeftCondition != null)
                {
                    if (biCond.LeftCondition is Condition)
                    {
                        ObjectParameter param;
                        leftExpr =
                            MakeComarisonExpression((Condition) biCond.LeftCondition, out param);
                        _objectParameters.Add(param);
                    }
                    else
                    {
                        leftExpr = MakeSingleCondition(biCond.LeftCondition);
                    }
                }

                if (biCond.RightCondition != null)
                {
                    if (biCond.RightCondition is Condition)
                    {
                        ObjectParameter param;
                        rightExpr =
                            MakeComarisonExpression((Condition) biCond.RightCondition, out param);
                        _objectParameters.Add(param);
                    }
                    else
                    {
                        rightExpr =
                            MakeSingleCondition(biCond.RightCondition);
                    }
                }

                if (leftExpr == null && rightExpr == null)
                {
                    return null;
                }

                if (leftExpr == null)
                {
                    body = rightExpr;
                }
                else if (rightExpr == null)
                {
                    body = leftExpr;
                }
                else
                {
                    if (biCond is OrCondition)
                    {
                        body = String.Format("({0}{1}{2})", leftExpr, PredicateConstants.OR, rightExpr);
                    }
                    else if (biCond is AndCondition)
                    {
                        body = String.Format("({0}{1}{2})", leftExpr, PredicateConstants.AND, rightExpr);
                    }
                }
            }

            return body;
        }

        private string MakeComarisonExpression(Condition condition, out ObjectParameter parameter)
        {
            string format;
            switch (condition.Operator)
            {
                case ConditionOperator.LessThen:
                    format = PredicateConstants.FieldLessThanValueFormat;
                    break;
                case ConditionOperator.LessOrEqualTo:
                    format = PredicateConstants.FieldLessThanEqualValueFormat;
                    break;
                case ConditionOperator.EqualTo:
                    format = PredicateConstants.FieldEqualValueFormat;
                    break;
                case ConditionOperator.GreaterOrEqualTo:
                    format = PredicateConstants.FieldGreateThanEqualValueFormat;
                    break;
                case ConditionOperator.GreaterThen:
                    format = PredicateConstants.FieldGreateThanValueFormat;
                    break;
                case ConditionOperator.StartsWith:
                case ConditionOperator.Like:
                case ConditionOperator.EndsWith:
                    format = PredicateConstants.FieldLikeValueFormat;
                    break;
                default:
                    throw new NotSupportedException("conditionOperator doesn't support");
            }

            string parameterName = String.Concat(condition.FieldName.Replace(".", String.Empty), _count++);
            string result = String.Format(format, TableName, condition.FieldName, parameterName);

            object value = condition.Value;
            switch (condition.Operator)
            {
                case ConditionOperator.StartsWith:
                    value = String.Concat(value, PredicateConstants.PerCentSign);
                    break;
                case ConditionOperator.Like:
                    value = String.Concat(PredicateConstants.PerCentSign, value, PredicateConstants.PerCentSign);
                    break;
                case ConditionOperator.EndsWith:
                    value = String.Concat(PredicateConstants.PerCentSign, value);
                    break;
            }

            parameter = new ObjectParameter(parameterName, value);

            return result;
        }

        public IExpressionMakerFactory Factory
        {
            get { throw new NotImplementedException(); }
        }

        public Expression Make(IEnumerable<BaseCondition> conditions, Expression sourceExpression)
        {
            throw new NotImplementedException();
        }

        public Expression MakeAggregate(Expression sourceExpression, ParameterExpression parameter, ConditionList conditionals, bool singleItem)
        {
            throw new NotImplementedException();
        }
    }
}