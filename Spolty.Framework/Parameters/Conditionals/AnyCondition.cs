using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public class AnyCondition : PredicateAggregationCondition
    {
        public AnyCondition()
        {
        }

        public AnyCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions) 
            : this(enumerableFieldName, conditions, true)
        {
        }
       
        public AnyCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, Type elementType) 
            : this(enumerableFieldName, conditions, true, elementType)
        {
        }
        
        public AnyCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, bool value) 
            : this(enumerableFieldName, conditions, value, null)
        {
        }
       
        public AnyCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, bool value, Type elementType) 
            : base(enumerableFieldName, conditions, value, ConditionOperator.EqualTo, elementType, AggregationMethod.Any)
        {
        }

        #region Overrides of BaseCondition

        public override object Clone()
        {
            return Clone<AnyCondition>();
        }

        #endregion
    }
}