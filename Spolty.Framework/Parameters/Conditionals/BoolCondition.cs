using System;
using System.Xml.Serialization;

namespace Spolty.Framework.Parameters.Conditionals
{
    /// <summary>
    /// Creates <see cref="BoolCondition"/>. 
    /// </summary>
    [Serializable]
    [XmlRoot("boolCondition")]
    public class BoolCondition : BaseCondition
    {
        private bool _value;

        /// <summary>
        /// Creates condition. Used only for serialization.
        /// </summary>
        public BoolCondition()
        {
        }

        /// <summary>
        /// Creates <see cref="BoolCondition"/>
        /// </summary>
        /// <param name="value"></param>
        public BoolCondition(bool value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets or sets value.
        /// </summary>
        [XmlAttribute("value")]
        public bool Value
        {
            get { return _value; }
            set { _value = value; }
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
            return new BoolCondition(_value);
        }
    }
}