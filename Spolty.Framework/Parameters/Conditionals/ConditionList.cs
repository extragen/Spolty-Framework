using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Spolty.Framework.Parameters.Conditionals
{
    [Serializable]
    [XmlRoot("conditions")]
    public class ConditionList : List<BaseCondition>, IParameterMarker
    {
        public ConditionList()
        {
        }

        public ConditionList(IEnumerable<BaseCondition> conditions)
        {
            AddConditions(conditions);
        }

        public ConditionList(params BaseCondition[] conditions)
        {
            AddConditions(conditions);
        }

        public int RemoveDuplicates()
        {
            return ListWrapperHelper.RemoveDuplicates(this);
        }

        public void AddConditions(IEnumerable<BaseCondition> conditions)
        {
            AddRange(conditions);
        }

        public void AddConditions(params BaseCondition[] conditions)
        {
            AddRange(conditions);
        }

        public void SetElementType(Type elementType)
        {
            foreach (BaseCondition condition in this)
            {
                if (condition is Condition)
                {
                    ((Condition) condition).ElementType = elementType;
                }
            }
        }

        public ConditionList ElementTypeConditions<T>()
        {
            return ElementTypeConditions(typeof (T));
        }

        public ConditionList ElementTypeConditions(Type elementType)
        {
            ConditionList lw = new ConditionList();
            foreach (Condition c in this)
            {
                if (c.ElementType != null && c.ElementType == elementType)
                {
                    lw.Add(c);
                }
            }
            return lw;
        }

        public ConditionList AggregateConditions(Type elementType)
        {
            ConditionList lw = new ConditionList();
            foreach (BaseCondition condition in this)
            {
                if (condition is AggregateCondition)
                {
                    AggregateCondition aggregateCondition = (AggregateCondition) condition;
                    if (aggregateCondition.ElementType != null &&
                        aggregateCondition.ElementType == elementType)
                    {
                        lw.Add(condition);
                    }
                }
            }
            return lw;
        }
    }
}