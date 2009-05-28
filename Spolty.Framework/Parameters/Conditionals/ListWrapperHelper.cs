using System.Collections.Generic;

namespace Spolty.Framework.Parameters.Conditionals
{
    internal class ListWrapperHelper
    {
        public static int RemoveDuplicates<T>(IList<T> collection)
        {
            if (collection.Count == 0)
            {
                return 0;
            }

            int res = 0;
            for (int i1 = collection.Count - 1; i1 >= 0; i1--)
            {
                T item = collection[i1];
                if (ContainsDuplicate(collection, item, i1))
                {
                    collection.RemoveAt(i1);
                    res++;
                }
            }
            return res;
        }

        private static bool ContainsDuplicate<T>(IList<T> collection, T item, int itemIndex)
        {
            for (int i2 = 0; i2 < collection.Count; i2++)
            {
                if (itemIndex == i2)
                {
                    break;
                }
                if (item.Equals(collection[i2]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}