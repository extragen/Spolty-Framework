using System;
using System.Xml.Serialization;

namespace Spolty.Framework.Parameters.Conditionals
{
    [Serializable]
    [XmlRoot("boolCondition")]
    public class BoolCondition : BaseCondition
    {
        private bool _value;


        public BoolCondition(bool _value)
        {
            this._value = _value;
        }

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