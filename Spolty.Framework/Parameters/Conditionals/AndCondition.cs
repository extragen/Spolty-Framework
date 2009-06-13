using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Checkers;
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
        private const int SizeInOneElement = 1;
        private const int SizeInTwoElements = 2;

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
        public AndCondition(IEnumerable<BaseCondition> conditions) : this()
        {
            CreateBiCondition<AndCondition>(conditions);
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