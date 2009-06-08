using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Aggregations;
using Spolty.Framework.Parameters.Aggregations.Enums;

namespace Spolty.Framework.Parameters.Aggregations
{
    public class AggregateMethod
    {
        protected readonly AggregateMethodType _aggregateMethodType;

        public AggregateMethodType AggregateMethodType
        {
            get { return _aggregateMethodType; }
        }

        internal AggregateMethod(AggregateMethodType aggregateMethodType, bool allTypes)
        {
            if (!allTypes)
            {
                if (aggregateMethodType != AggregateMethodType.None &&
                    aggregateMethodType != AggregateMethodType.Count &&
                    aggregateMethodType != AggregateMethodType.LongCount)
                {
                    throw new SpoltyException("Not supported AggregateMethodType");
                }
            }
            _aggregateMethodType = aggregateMethodType;
        }

        public AggregateMethod(AggregateMethodType aggregateMethodType) : this(aggregateMethodType, false)
        {
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
                    return AggregateMethodType == val2.AggregateMethodType;
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
            int result = _aggregateMethodType.GetHashCode();
            return result;
        }
    }
}