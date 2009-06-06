using System;
using System.Xml.Serialization;
using Spolty.Framework.Parameters.Conditionals.Enums;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Class define condition
    /// </summary>
    [Serializable]
    [XmlRoot("condition")]
    public class Condition : BaseCondition
    {
        #region Fields

        private string _fieldName;
        private object _value;
        private Type _elementType;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates condition. Used only for serialization.
        /// </summary>
        public Condition()
        {
        }

        /// <summary>
        /// Creates condition with <see cref="ConditionOperator.EqualTo"/> and not defined <see cref="ElementType"/>.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table</param>
        /// <param name="value"></param>
        public Condition(string fieldName, object value)
            : this(fieldName, value, ConditionOperator.EqualTo, null)
        {
        }

        /// <summary>
        /// Creates condition with not defined <see cref="ElementType"/>. 
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="value"></param>
        /// <param name="condOperator"></param>
        public Condition(string fieldName, object value, ConditionOperator condOperator)
            : this(fieldName, value, condOperator, null)
        {
        }

        /// <summary>
        /// Creates condition to concrete <see cref="ElementType"/>.
        /// </summary>
        /// <param name="fieldName">name of a field which mapped with table.</param>
        /// <param name="value"></param>
        /// <param name="condOperator"></param>
        /// <param name="elementType">defines filtering Entity.</param>
        public Condition(string fieldName, object value, ConditionOperator condOperator, Type elementType)
            : base(condOperator)
        {
            _fieldName = fieldName;
            _value = value;
            _elementType = elementType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets field name
        /// </summary>
        [XmlAttribute("fieldName")]
        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        /// <summary>
        /// Gets or sets value for comparison
        /// </summary>
        [XmlAttribute("value")]
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets filtering entity type
        /// </summary>
        [XmlAttribute("elementType")]
        public Type ElementType
        {
            get { return _elementType; }
            set { _elementType = value; }
        }

        #endregion

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int result = _fieldName.GetHashCode();
            result ^= Operator.GetHashCode();
            if (_value != null)
            {
                result ^= _value.GetHashCode();
            }
            if (_elementType != null)
            {
                result ^= _elementType.GetHashCode();
            }
            return result;
        }


        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        ///</returns>
        ///
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            Condition val2 = obj as Condition;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    if (_value != null)
                    {
                        return _fieldName == val2._fieldName &&
                               Operator == val2.Operator &&
                               _value.Equals(val2._value) &&
                               _elementType == val2._elementType;
                    }
                    else
                    {
                        return _fieldName == val2._fieldName &&
                               Operator == val2.Operator &&
                               val2._value == null &&
                               _elementType == val2._elementType;
                    }
                }
            }
            return false;
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
            return new Condition(FieldName, Value, Operator, ElementType);
        }
    }
}