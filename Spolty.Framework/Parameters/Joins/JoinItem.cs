using System;
using System.Collections.Generic;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.Parameters.Joins
{
    [Serializable]
    public class JoinItem
    {
        private Item _left;
        private Item _right;
        private JoinType _joinType;

        public Item Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public Item Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public JoinType JoinType
        {
            get { return _joinType; }
            set { _joinType = value; }
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject)
            : this(leftBizObjectType, leftKeyName, rightBizObject, leftKeyName)
        {
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject, string rightKeyName)
        {
            _left = new Item(leftBizObjectType, leftKeyName);
            _right = new Item(rightBizObject, rightKeyName);
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject, JoinType joinType)
            : this(leftBizObjectType, leftKeyName, rightBizObject)

        {
            _joinType = joinType;
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject, string rightKeyName,
                        JoinType joinType)
            : this(leftBizObjectType, leftKeyName, rightBizObject, rightKeyName)
        {
            _joinType = joinType;
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject, string rightKeyName,
                        JoinType joinType, IEnumerable<string> leftFields, IEnumerable<string> rightFields)
            : this(leftBizObjectType, leftKeyName, rightBizObject, rightKeyName, leftFields, rightFields)
        {
            _joinType = joinType;
        }

        public JoinItem(Type leftBizObjectType, string leftKeyName, Type rightBizObject, string rightKeyName, IEnumerable<string> leftFields, IEnumerable<string> rightFields)
        {
            _left = new Item(leftBizObjectType, leftKeyName, leftFields);
            _right = new Item(rightBizObject, rightKeyName, rightFields);

            if (_left.Fields.Count != _right.Fields.Count)
            {
                throw new SpoltyException("Number fields mismatch");
            }
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
            JoinItem val2 = obj as JoinItem;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _left.Equals(val2._left) && _right.Equals(val2._right);
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
            int result = _left.GetHashCode();
            result ^= _right.GetHashCode();
            return result;
        }

        public static bool operator ==(JoinItem val1, JoinItem val2)
        {
            return Equals(val1, val2);
        }

        public static bool operator !=(JoinItem val1, JoinItem val2)
        {
            return !(val1 == val2);
        }
    }
}