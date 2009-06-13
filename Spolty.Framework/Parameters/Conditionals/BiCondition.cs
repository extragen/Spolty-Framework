using System;
using System.Collections.Generic;
using Spolty.Framework.Checkers;

namespace Spolty.Framework.Parameters.Conditionals
{
    [Serializable]
    public abstract class BiCondition : BaseCondition
    {
        private BaseCondition _leftCondition;
        private BaseCondition _rightCondition;

        public BiCondition()
        {
        }

        public BiCondition(BaseCondition leftCondition, BaseCondition rightCondition)
        {
            _leftCondition = leftCondition;
            _rightCondition = rightCondition;
        }

        /// <summary>
        /// Gets left <see cref="BaseCondition"/>
        /// </summary>
        public BaseCondition LeftCondition
        {
            get { return _leftCondition; }
            set { _leftCondition = value; }
        }

        /// <summary>
        /// Gets right <see cref="BaseCondition"/>
        /// </summary>
        public BaseCondition RightCondition
        {
            get { return _rightCondition; }
            set { _rightCondition = value; }
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
            int result = Operator.GetHashCode();
            if (_leftCondition != null)
            {
                result ^= _leftCondition.GetHashCode();
            }
            if (_rightCondition != null)
            {
                result ^= _rightCondition.GetHashCode();
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
            BiCondition val2 = obj as BiCondition;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return
                        _leftCondition == val2.LeftCondition && Operator == val2.Operator &&
                        _rightCondition.Equals(val2.RightCondition);
                }
            }
            return false;
        }

        protected void CreateBiCondition<T>(IEnumerable<BaseCondition> conditions) where T : BiCondition, new()
        {
            Checker.CheckArgumentNull(conditions, "conditions");
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
                    RightCondition = new T { LeftCondition = condition, RightCondition = RightCondition };
                }
            }
        }
    }
}