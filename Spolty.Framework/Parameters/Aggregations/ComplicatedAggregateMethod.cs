using System.Reflection;
using Spolty.Framework.Parameters.Aggregations.Enums;

namespace Spolty.Framework.Parameters.Aggregations
{
    public class ComplicatedAggregateMethod : AggregateMethod
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        public ComplicatedAggregateMethod(AggregateMethodType aggregateMethodType, PropertyInfo propertyInfo)
            : base(aggregateMethodType, true)
        {
            _propertyInfo = propertyInfo;
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
            ComplicatedAggregateMethod val2 = obj as ComplicatedAggregateMethod;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return AggregateMethodType == val2.AggregateMethodType && _propertyInfo == val2._propertyInfo;
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
            int result = base.GetHashCode();
            result ^= _propertyInfo.GetHashCode();
            return result;
        }
    }
}