using System;
using System.Xml.Serialization;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class defines FieldConditions which makes possible to compare columns between to tables.
    /// The <see cref="FieldCondition"/> using for joins between tables by fields.
    /// </summary>
    [Serializable]
    public class FieldCondition : BaseCondition
    {
        #region Fields

        private string _leftFieldName;
        private string _rightFieldName;
        private Type _leftElementType;
        private Type _rightElementType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets field name for the <see cref="LeftElementType"/>.
        /// </summary>
        [XmlAttribute("leftField")]
        public string LeftFieldName
        {
            get { return _leftFieldName; }
            set { _leftFieldName = value; }
        }

        /// <summary>
        /// Get or sets field name for the <see cref="RightElementType"/>.
        /// </summary>
        [XmlAttribute("rightField")]
        public string RightFieldName
        {
            get { return _rightFieldName; }
            set { _rightFieldName = value; }
        }

        /// <summary>
        /// Gets or sets elment type for the left table.
        /// </summary>
        [XmlAttribute("leftElementType")]
        public Type LeftElementType
        {
            get { return _leftElementType; }
            set { _leftElementType = value; }
        }

        /// <summary>
        /// Gets or sets elment type for the right table.
        /// </summary>
        [XmlAttribute("rightElementType")]
        public Type RightElementType
        {
            get { return _rightElementType; }
            set { _rightElementType = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates condition. Used only for serialization.
        /// </summary>
        public FieldCondition()
        {
        }

        /// <summary>
        /// Creates <see cref="FieldCondition"/> 
        /// </summary>
        /// <param name="leftFieldName">field name for the left Entity.</param>
        /// <param name="rightFieldName">field name for the right Entity</param>
        /// <param name="condOperator"></param>
        /// <param name="leftElementType">type of left Entity.</param>
        /// <param name="rightElementType">type of right Entity.</param>
        public FieldCondition(string leftFieldName, ConditionOperator condOperator, string rightFieldName, Type leftElementType, Type rightElementType)
            : base(condOperator)
        {
            _leftFieldName = leftFieldName;
            _rightFieldName = rightFieldName;
            _leftElementType = leftElementType;
            _rightElementType = rightElementType;
        }

        #endregion

        public override bool Equals(object obj)
        {
            FieldCondition val2 = obj as FieldCondition;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _leftFieldName.Equals(val2._leftFieldName) &&
                           _rightFieldName.Equals(val2._rightFieldName) &&
                           Operator == val2.Operator &&
                           _leftElementType == val2._leftElementType &&
                           _rightElementType == val2._rightElementType;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result ^= _leftFieldName.GetHashCode();
            result ^= _rightFieldName.GetHashCode();
            result ^= _leftElementType.GetHashCode();
            result ^= _rightElementType.GetHashCode();
            return result;
        }

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override object Clone()
        {
            return new FieldCondition(LeftFieldName, Operator, RightFieldName, LeftElementType, RightElementType);
        }
    }
}