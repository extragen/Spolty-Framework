using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public abstract class PredicateAggregationCondition : BaseAggregationCondition
    {
        private readonly ConditionList _conditions;

        protected PredicateAggregationCondition(){}


        protected PredicateAggregationCondition(string enumerableFieldName, 
                                                IEnumerable<BaseCondition> conditions, 
                                                object value, 
                                                ConditionOperator conditionOperator,
                                                Type elementType, 
                                                AggregationMethod aggregationMethod) 
            : base(conditionOperator)
        {
            EnumerableFieldName = enumerableFieldName;
            _conditions = new ConditionList();
            if (conditions != null)
            {
                _conditions.AddConditions(conditions);
            }
            Value = value;
            _elementType = elementType;
            AggregationMethod = aggregationMethod;
        }

        public ConditionList Conditions
        {
            get { return _conditions; }
        }

        protected object Clone<T>() where T : PredicateAggregationCondition, new()
        {
            T clone = new T
                          {
                              EnumerableFieldName = _enumerableFieldName,
                              ElementType = _elementType,
                              Value = Value,
                              Operator = Operator
                          };
            clone.Conditions.AddConditions(Conditions);
            return clone;
        }
    }
}