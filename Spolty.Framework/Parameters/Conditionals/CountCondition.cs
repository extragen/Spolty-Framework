using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public class CountCondition : PredicateAggregationCondition
    {
        public CountCondition()
        {
        }

        public CountCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, int value) 
            : this(enumerableFieldName, conditions, value, ConditionOperator.EqualTo, null)
        {
        }
        
        public CountCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, int value, ConditionOperator conditionOperator)
            : this(enumerableFieldName, conditions, value, conditionOperator, null)
        {
        }
       
        public CountCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, int value, ConditionOperator conditionOperator, Type elementType) 
            : base(enumerableFieldName, conditions, value, conditionOperator, elementType, AggregationMethod.Count)
        {
        }

        #region Overrides of BaseCondition

        public override object Clone()
        {
            return Clone<CountCondition>();
        }

        #endregion
    }
}