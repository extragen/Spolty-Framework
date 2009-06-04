using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Exceptions;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define AndCondition. 
    /// </summary>
    [Serializable]
    [XmlRoot("andCondition")]
    public class AndCondition : BiCondition
    {
        #region Constructors

        /// <summary>
        /// Creates condition. Used only for serialization.
        /// </summary>
        public AndCondition()
        {
        }

        /// <summary>
        /// Creates <see cref="AndCondition"/> between left and right <see cref="BaseCondition"/>.
        /// </summary>
        /// <param name="leftCondition">left condition.</param>
        /// <param name="rightCondition">right condition.</param>
        public AndCondition(BaseCondition leftCondition, BaseCondition rightCondition)
            : base(leftCondition, rightCondition)
        {
        }

        /// <summary>
        /// Creates <see cref="AndCondition"/> by <see cref="IList{T}"/> of conditions.
        /// </summary>
        /// <param name="conditions"></param>
        public AndCondition(IList<BaseCondition> conditions) : this()
        {
            if (conditions == null || conditions.Count == 0)
            {
                throw new SpoltyException("conditions is incorrect");
            }

            LeftCondition = conditions[0];

            BaseCondition rightCondition = null;

            if (conditions.Count == 1)
            {
                return;
            }
            
            if (conditions.Count >= 2)
            {
                rightCondition = conditions[1];
            }

            for (int index = 2; index < conditions.Count; index++)
            {
                rightCondition = new AndCondition(rightCondition, conditions[index]);
            }

            RightCondition = rightCondition;
        }

        #endregion

        public override object Clone()
        {
            return
                new AndCondition(LeftCondition != null ? (BaseCondition) LeftCondition.Clone() : null,
                                 RightCondition != null ? (BaseCondition) RightCondition.Clone() : null);
        }
    }
}