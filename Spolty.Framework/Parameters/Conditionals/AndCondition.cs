using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Exceptions;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define condition
    /// </summary>
    [Serializable]
    [XmlRoot("andCondition")]
    public class AndCondition : BiCondition
    {
        #region Fields

        #endregion

        #region Constructors

        public AndCondition()
        {
        }


        public AndCondition(BaseCondition leftCondition, BaseCondition rightCondition)
            : base(leftCondition, rightCondition)
        {
        }

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
            else if (conditions.Count >= 2)
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

        #region Properties

        #endregion

        public override object Clone()
        {
            return
                new AndCondition(LeftCondition != null ? (BaseCondition) LeftCondition.Clone() : null,
                                 RightCondition != null ? (BaseCondition) RightCondition.Clone() : null);
        }
    }
}