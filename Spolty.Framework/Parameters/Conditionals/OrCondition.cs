using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define OrConditions
    /// </summary>
    [Serializable]
    [XmlRoot("orCondition")]
    public class OrCondition : BiCondition
    {
        /// <summary>
        /// Creates condition. Used only for serialization.
        /// </summary>
        public OrCondition()
        {
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> between left and right <see cref="BaseCondition"/>s.
        /// </summary>
        /// <param name="leftCondition">left condition.</param>
        /// <param name="rightCondition">right condition.</param>
        public OrCondition(BaseCondition leftCondition, BaseCondition rightCondition)
            : base(leftCondition, rightCondition)
        {
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> between left and right <see cref="IList{T}"/>s.
        /// List of conditions in parameters unite inside by <see cref="AndCondition"/>.
        /// </summary>
        /// <param name="leftConditions">conditions which will be created as <see cref="AndCondition"/>.</param>
        /// <param name="rightConditions">conditions which will be created as <see cref="AndCondition"/>.</param>
        public OrCondition(IList<BaseCondition> leftConditions, IList<BaseCondition> rightConditions) : this()
        {
            if (leftConditions == null || leftConditions.Count == 0)
            {
                throw new SpoltyException("leftConditions is incorrect");
            }

            if (rightConditions == null)
            {
                throw new SpoltyException("rightConditions is incorrect");
            }

            LeftCondition = new AndCondition(leftConditions);

            if (rightConditions.Count > 0)
            {
                RightCondition = new AndCondition(rightConditions);
            }
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> from <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="conditions"><see cref="IEnumerable{T}"/> of conditions.</param>
        public OrCondition(IEnumerable<BaseCondition> conditions)
        {
            if (conditions == null)
            {
                throw new SpoltyException("conditions is incorrect");
            }

            foreach (BaseCondition condition in conditions)
            {
                if (LeftCondition == null)
                {
                    LeftCondition = condition;
                }
                else if (RightCondition == null)
                {
                    RightCondition = condition;
                }
                else
                {
                    RightCondition = new OrCondition(condition, RightCondition);
                }
            }
        }

        public override object Clone()
        {
            return
                new OrCondition(LeftCondition != null ? (BaseCondition) LeftCondition.Clone() : null,
                                RightCondition != null ? (BaseCondition) RightCondition.Clone() : null);
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> with the same <paramref name="fieldName"/> cref="fieldName"/> and differents <paramref name="values"/>.
        /// <paramref name="fieldName"/> and <paramref name="values"/> compare by <see cref="ConditionOperator.EqualTo"/>.
        /// <see cref="Condition.ElementType"/> define as null.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        /// <returns><see cref="OrCondition"/> which contains <see cref="Condition"/> where the same 
        /// <paramref name="fieldName"/> and differents <paramref name="values"/>.
        /// </returns>
        public static OrCondition Create(string fieldName, object[] values)
        {
            return Create(fieldName, values, ConditionOperator.EqualTo, null);
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> with the same <paramref name="fieldName"/> cref="fieldName"/> and differents <paramref name="values"/>.
        /// <see cref="Condition.ElementType"/> define as null.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        /// <param name="operator"></param>
        /// <returns><see cref="OrCondition"/> which contains <see cref="Condition"/> where the same 
        /// <paramref name="fieldName"/> and differents <paramref name="values"/>.
        /// </returns>
        public static OrCondition Create(string fieldName, object[] values, ConditionOperator @operator)
        {
            return Create(fieldName, values, @operator, null);
        }

        /// <summary>
        /// Creates <see cref="OrCondition"/> with the same <paramref name="fieldName"/> cref="fieldName"/> and differents <paramref name="values"/>.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="values"></param>
        /// <param name="operator"></param>
        /// <param name="entityType">defines filtering Entity.</param>
        /// <returns><see cref="OrCondition"/> which contains <see cref="Condition"/> where the same 
        /// <paramref name="fieldName"/> and differents <paramref name="values"/>.
        /// </returns>
        public static OrCondition Create(string fieldName, object[] values, ConditionOperator @operator, Type entityType)
        {
            Checker.CheckArgumentNull(fieldName, "fieldName");

            if (values == null || values.Length == 0)
            {
                throw new ArgumentNullException("values");
            }

            var conditionList = new ConditionList();
            conditionList.AddConditions(fieldName, values, @operator, entityType);

            return new OrCondition(conditionList);
        }
    }
}