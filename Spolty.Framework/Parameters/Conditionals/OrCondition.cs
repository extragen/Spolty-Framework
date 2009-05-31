using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define condition
    /// </summary>
    [Serializable]
    [XmlRoot("orCondition")]
    public class OrCondition : BiCondition
    {
        #region Fields

        #endregion

        #region Constructors

        public OrCondition()
        {
        }

        public OrCondition(BaseCondition leftCondition, BaseCondition rightCondition)
            : base(leftCondition, rightCondition)
        {
        }

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

        #endregion

        #region Properties

        #endregion

        public override object Clone()
        {
            return
                new OrCondition(LeftCondition != null ? (BaseCondition) LeftCondition.Clone() : null,
                                RightCondition != null ? (BaseCondition) RightCondition.Clone() : null);
        }

        public static OrCondition Create(string fieldName, object[] values)
        {
            return Create(fieldName, values, ConditionOperator.EqualTo, null);
        }

        public static OrCondition Create(string fieldName, object[] values, ConditionOperator @operator)
        {
            return Create(fieldName, values, @operator, null);
        }
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