using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define list of <see cref="BaseCondition"/>.
    /// Class markered as <see cref="IParameterMarker"/> for using it as passing parameter.
    /// </summary>
    [Serializable]
    [XmlRoot("conditions")]
    public class ConditionList : List<BaseCondition>, IParameterMarker
    {
        /// <summary>
        /// Creates empty condition list
        /// </summary>
        public ConditionList()
        {
        }

        /// <summary>
        /// Creates condition list by <see cref="IEnumerable{BaseCondition}"/>
        /// </summary>
        /// <param name="conditions"></param>
        public ConditionList(IEnumerable<BaseCondition> conditions)
        {
            AddConditions(conditions);
        }

        /// <summary>
        /// Creates condtion list by params 
        /// </summary>
        /// <param name="conditions"></param>
        public ConditionList(params BaseCondition[] conditions)
        {
            AddConditions(conditions);
        }

        /// <summary>
        /// Removes duplicates condition
        /// </summary>
        /// <returns>number of removed items</returns>
        public int RemoveDuplicates()
        {
            return ListWrapperHelper.RemoveDuplicates(this);
        }

        /// <summary>
        /// Adds condtions to conditions list
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(IEnumerable<BaseCondition> conditions)
        {
            AddRange(conditions);
        }

        /// <summary>
        /// Adds condtions to conditions list
        /// </summary>
        /// <param name="conditions"></param>
        public void AddConditions(params BaseCondition[] conditions)
        {
            AddRange(conditions);
        }

        /// <summary>
        /// Sets concrete type to all condition 
        /// </summary>
        /// <param name="elementType"></param>
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

        /// <summary>
        /// Gets condtion list with concrete type defined in generic.
        /// </summary>
        /// <typeparam name="T">concrete type.</typeparam>
        /// <returns>condition list with type of <see cref="T"/>.</returns>
        public ConditionList ElementTypeConditions<T>()
        {
            return ElementTypeConditions(typeof (T));
        }

        /// <summary>
        /// Gets condtion list with concrete type
        /// </summary>
        /// <param name="elementType">concrete type.</param>
        /// <returns>condtion list with concrete type.</returns>
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

        /// <summary>
        /// Adds <see cref="Condition"/>s with the same <see cref="fieldName"/> and differents <see cref="values"/>.
        /// <see cref="fieldName"/> and <see cref="values"/> compare by <see cref="ConditionOperator.EqualTo"/>.
        /// <see cref="Condition.ElementType"/> define as null.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        public void AddConditions(string fieldName, object[] values)
        {
            AddConditions(fieldName, values, ConditionOperator.EqualTo, null);
        }

        /// <summary>
        /// Adds <see cref="Condition"/>s with the same <see cref="fieldName"/> and differents <see cref="values"/>
        /// <see cref="Condition.ElementType"/> define as null.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        /// <param name="operator"></param>
        public void AddConditions(string fieldName, object[] values,
                                                            ConditionOperator @operator)
        {
            AddConditions(fieldName, values, @operator, null);
        }

        /// <summary>
        /// Adds <see cref="Condition"/>s with the same <see cref="fieldName"/> and differents <see cref="values"/>
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        /// <param name="operator"></param>
        /// <param name="entityType">defines filtering Entity.</param>
        public void AddConditions(string fieldName, object[] values, ConditionOperator @operator, Type entityType)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (values == null || values.Length == 0)
            {
                throw new ArgumentNullException("values");
            }

            foreach (object value in values)
            {
                Add(new Condition(fieldName, value, @operator, entityType));
            }
        }

    }
}