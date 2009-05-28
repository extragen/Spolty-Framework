using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Spolty.Framework.Parameters.Joins
{
    public class Item
    {
        private readonly Type _bizObjectType;
        private readonly string _propertyName;
        private readonly List<string> _fields = new List<string>();

        public Type BizObjectType
        {
            get { return _bizObjectType; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }
		
        public ReadOnlyCollection<string> Fields
        {
            get { return _fields.AsReadOnly(); }
        }

        public Item(Type bizObjectType) : this(bizObjectType, bizObjectType.Name)
        {
        }

        public Item(Type bizObjectType, string propertyName)
        {
            _bizObjectType = bizObjectType;
            _propertyName = propertyName;
        }

        public Item(Type bizObjectType, string propertyName, IEnumerable<string> fields) : this(bizObjectType, propertyName)
        {
            _fields.AddRange(fields);
            _fields.TrimExcess();
        }

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            Item val2 = obj as Item;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _bizObjectType == val2._bizObjectType && _propertyName == val2._propertyName;
                }
            }
            return false;
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int result = _bizObjectType.GetHashCode();
            result ^= _propertyName.GetHashCode();
            return result;
        }
    }
}