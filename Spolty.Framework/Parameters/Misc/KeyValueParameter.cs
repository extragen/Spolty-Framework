using System;

namespace Spolty.Framework.Parameters.Misc
{
    [Serializable]
    public class KeyValueParameter
    {
        private string _key;
        private object _value;

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public KeyValueParameter()
        {
        }

        public KeyValueParameter(string key, object value)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException();
            }
            _key = key;
            _value = value;
        }

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
            int result = _key.GetHashCode();
            if (_value != null)
            {
                result ^= _value.GetHashCode();
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
            KeyValueParameter val2 = obj as KeyValueParameter;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _key == val2._key && Equals(_value, val2._value);
                }
            }
            return false;
        }

        public static bool operator ==(KeyValueParameter val1, KeyValueParameter val2)
        {
            return Equals(val1, val2);
        }

        public static bool operator !=(KeyValueParameter val1, KeyValueParameter val2)
        {
            return ! (val1 == val2);
        }
    }
}