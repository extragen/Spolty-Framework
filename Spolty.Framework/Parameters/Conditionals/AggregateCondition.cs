using System;
using System.Xml.Serialization;
using Spolty.Framework.Parameters.Aggregations.Enums;
using Spolty.Framework.Parameters.Conditionals.Enums;
using Spolty.Framework.Parameters.Misc;

namespace Spolty.Framework.Parameters.Conditionals
{
    [Serializable]
    [XmlRoot("aggregateCondition")]
    public class AggregateCondition : BaseCondition
    {
        #region Fields

        private AggregateMethodType _aggregateMethodType;
        private string _fieldName;
        private object _value;
        private Type _elementType;

        #endregion

        #region Properties

        [XmlAttribute("aggregateMethod")]
        public AggregateMethodType AggregateMethodType
        {
            get { return _aggregateMethodType; }
            set { _aggregateMethodType = value; }
        }

        [XmlAttribute("fieldName")]
        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        [XmlAttribute("value")]
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        [XmlAttribute("elementType")]
        public Type ElementType
        {
            get { return _elementType; }
            set { _elementType = value; }
        }

        #endregion

        #region Constructors

        public AggregateCondition()
        {
        }

        public AggregateCondition(AggregateMethodType aggregateMethodType, string fieldName, object value,
                                  ConditionOperator condOperator)
            : this(aggregateMethodType, fieldName, value, condOperator, null)
        {
        }


        public AggregateCondition(AggregateMethodType aggregateMethodType, object value, Type elementType)
        {
            if (aggregateMethodType != AggregateMethodType.Count && aggregateMethodType != AggregateMethodType.LongCount)
            {
                throw new NotSupportedException();
            }
            _aggregateMethodType = aggregateMethodType;
            _value = value;
            _elementType = elementType;
        }

        public AggregateCondition(AggregateMethodType aggregateMethodType, string fieldName, object value,
                                  ConditionOperator condOperator, Type elementType) : base(condOperator)
        {
            _aggregateMethodType = aggregateMethodType;
            _fieldName = fieldName;
            _value = value;
            _elementType = elementType;
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
            int result = base.GetHashCode();
            result ^= AggregateMethodType.GetHashCode();
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
            AggregateCondition val2 = obj as AggregateCondition;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return
                        _aggregateMethodType == val2._aggregateMethodType && Operator == val2.Operator &&
                        Value.Equals(val2.Value) && ElementType == val2.ElementType;
                }
            }
            return false;
        }

        public static implicit operator KeyValueParameter(AggregateCondition item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return new KeyValueParameter(item.FieldName, item.Value);
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
            return new AggregateCondition(AggregateMethodType, FieldName, Value, Operator, ElementType);
        }
    }
}