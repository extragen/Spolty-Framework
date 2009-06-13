using System;
using Spolty.Framework.Checkers;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public abstract class BaseAggregationCondition : BaseCondition
    {
        protected string _enumerableFieldName;
        protected Type _elementType;
        private object _value;
        internal AggregationMethod AggregationMethod { get; set; }

        public string EnumerableFieldName
        {
            get { return _enumerableFieldName; }
            set
            {
                Checker.CheckStringArgumentNull(value, "EnumerableFieldName");
                _enumerableFieldName = value;
            }
        }

        public Type ElementType
        {
            get { return _elementType; }
            set { _elementType = value; }
        }

        public object Value
        {
            get { return _value; }
            set
            {
                Checker.CheckArgumentNull(value, "Value");
                _value = value;
            }
        }

        protected BaseAggregationCondition()
        {
        }

        protected BaseAggregationCondition(ConditionOperator _operator) : base(_operator)
        {
        }
    }
}