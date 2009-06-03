using System;
using System.Collections.Generic;
using System.Text;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Orderings.Enums;

namespace Spolty.Framework.Parameters.Orderings
{
    public class OrderingList : List<Ordering>, IParameterMarker
    {
        public OrderingList()
        {
        }

        public OrderingList(params Ordering[] orderings)
        {
            AddOrderings(orderings);
        }
        
        public OrderingList(IEnumerable<Ordering> orderings)
        {
            AddOrderings(orderings);
        }

        public OrderingList(string sortExpression)
        {
            AddRange(CreateSortOrderingListWrapper(sortExpression, String.Empty));
        }

        public int RemoveDuplicates()
        {
            return ListWrapperHelper.RemoveDuplicates(this);
        }

        public void AddOrderings(params Ordering[] orderings)
        {
            AddRange(orderings);
        }

        public void AddOrderings(IEnumerable<Ordering> orderings)
        {
            AddRange(orderings);
        }

        private static OrderingList CreateSortOrderingListWrapper(string sortExpression, string defaultColumn)
        {
            if (string.IsNullOrEmpty(sortExpression) || (sortExpression.Trim().Length == 0))
            {
                if (defaultColumn.Length > 0 && defaultColumn.Split(',').Length > 1)
                    return CreateSortOrderingListWrapper(defaultColumn, String.Empty);
                var list = new OrderingList();
                list.Add(new Ordering(defaultColumn, SortDirection.Ascending));
                return list;
            }

            string[] paramArray = sortExpression.Trim().Split(new[] {','});
            var list2 = new OrderingList();
            foreach (string par in paramArray)
            {
                string[] array = par.Trim().Split(new[] {' '});
                string strSort = string.Empty;
                SortDirection direction = SortDirection.Ascending;
                if (array.Length > 0)
                {
                    strSort = array[0];
                }
                if ((array.Length > 1) && string.Equals(array[1], "DESC", StringComparison.OrdinalIgnoreCase))
                {
                    direction = SortDirection.Descending;
                }
                list2.Add(new Ordering(strSort, direction));
            }
            return list2;
        }

        ///<summary>
        ///Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            const string AscedingFormat = "{0}, ";
            const string DescendingFormat = "{0} Desc, ";

            var orderings = new StringBuilder();

            foreach (Ordering ordering in this)
            {
                orderings.AppendFormat(
                    ordering.SortDirection == SortDirection.Ascending ? AscedingFormat : DescendingFormat,
                    ordering.ColumnName);
            }
            orderings.Length -= 2;

            return orderings.ToString();
        }
    }
}