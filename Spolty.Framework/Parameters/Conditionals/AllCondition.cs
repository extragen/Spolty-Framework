using System;
using System.Collections.Generic;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    public enum AggregationMethod
    {
        All,
        Any,
        Average,
        Count,
        Max,
        Min,
        Sum
    }

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