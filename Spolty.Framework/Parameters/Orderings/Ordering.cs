using System;
using Spolty.Framework.Parameters.Orderings.Enums;

namespace Spolty.Framework.Parameters.Orderings
{
    /// <summary>
    /// Ordering definition.
    /// Defines ordering rule for one column
    /// </summary>
    public class Ordering
    {
        #region Fields

        private string _columnName;
        private SortDirection _sortDirection;
        private Type _elementType;

        #endregion

        #region Constructors

        public Ordering()
        {
        }

        /// <summary>
        /// Create ordering with ascending direction.
        /// </summary>
        /// <param name="columnName"></param>
        public Ordering(string columnName) : this(columnName, SortDirection.Ascending)
        {
            _columnName = columnName;
        }

        /// <summary>
        /// Create ordering with ascending direction.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="elementType"></param>
        public Ordering(string columnName, Type elementType) : this(columnName, SortDirection.Ascending, elementType)
        {
            _columnName = columnName;
            _elementType = elementType;
        }

        public Ordering(string columnName, SortDirection sortDirection)
        {
            _columnName = columnName;
            _sortDirection = sortDirection;
        }

        public Ordering(string columnName, SortDirection sortDirection, Type elementType)
            : this(columnName, sortDirection)
        {
            _elementType = elementType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets field name
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// Gets or sets comparison operator
        /// </summary>
        public SortDirection SortDirection
        {
            get { return _sortDirection; }
            set { _sortDirection = value; }
        }

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
            int result = _columnName.GetHashCode();
            result ^= _sortDirection.GetHashCode();
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
            Ordering val2 = obj as Ordering;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _columnName == val2._columnName && _sortDirection == val2._sortDirection;
                }
            }
            return false;
        }

        public static bool operator ==(Ordering val1, Ordering val2)
        {
            return Equals(val1, val2);
        }

        public static bool operator !=(Ordering val1, Ordering val2)
        {
            return ! (val1 == val2);
        }
    }
}