using System;
using System.Xml.Serialization;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    [Serializable]
    [XmlInclude(typeof (Condition))]
    [XmlInclude(typeof (OrCondition))]
    [XmlInclude(typeof (AndCondition))]
    [XmlInclude(typeof (BoolCondition))]
    [XmlInclude(typeof (FieldCondition))]
    public abstract class BaseCondition : IEquatable<BaseCondition>, ICloneable
    {
        private ConditionOperator _operator;

        public BaseCondition()
        {
        }

        public BaseCondition(ConditionOperator _operator)
        {
            this._operator = _operator;
        }

        /// <summary>
        /// Gets or sets comparison operator
        /// </summary>
        [XmlAttribute("operator")]
        public ConditionOperator Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public static bool operator ==(BaseCondition val1, BaseCondition val2)
        {
            return Equals(val1, val2);
        }

        public static bool operator !=(BaseCondition val1, BaseCondition val2)
        {
            return ! (val1 == val2);
        }

        bool IEquatable<BaseCondition>.Equals(BaseCondition baseCondition)
        {
            if (baseCondition == null)
            {
                return false;
            }
            if (ReferenceEquals(this, baseCondition))
            {
                return true;
            }
            return baseCondition.Equals(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as BaseCondition);
        }

        public override int GetHashCode()
        {
            return _operator.GetHashCode();
        }

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public abstract object Clone();

        #endregion
    }
}