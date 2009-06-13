using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public class AllCondition : PredicateAggregationCondition
    {
        public AllCondition()
        {
        }

        public AllCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions) 
            : this(enumerableFieldName, conditions, true)
        {
        }
       
        public AllCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, Type elementType) 
            : this(enumerableFieldName, conditions, true, elementType)
        {
        }
        
        public AllCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, bool value) 
            : this(enumerableFieldName, conditions, value, null)
        {
        }

        public AllCondition(string enumerableFieldName, IEnumerable<BaseCondition> conditions, bool value, Type elementType) 
            : base(enumerableFieldName, conditions, value, ConditionOperator.EqualTo, elementType, AggregationMethod.All)
        {
        }

        #region Overrides of BaseCondition

        public override object Clone()
        {
            return Clone<AllCondition>();
        }

        #endregion
    }
}