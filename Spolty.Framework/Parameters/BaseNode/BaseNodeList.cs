using System;
using System.Collections.Generic;

namespace Spolty.Framework.Parameters.BaseNode
{
    public class BaseNodeList : List<BaseNode>
    {
        private readonly BaseNode _parentNode;

        public BaseNodeList(BaseNode parentNode)
        {
            _parentNode = parentNode;
        }

        public BaseNode ParentNode
        {
            get { return _parentNode; }
        }

        public BaseNode this[Type bizObjectType]
        {
            get { return ParentNode.FindInChildren(bizObjectType); }
        }

        public new virtual void Add(BaseNode childNode)
        {
            childNode._parentNode = ParentNode;
            childNode.Level = ParentNode.Level + 1;
            if (Count == 0)
            {
                (this as List<BaseNode>).Add(childNode);
            }
            else
            {
                int searchIndex = BinarySearch(childNode);
                if (searchIndex >= 0)
                {
                    throw new ArgumentException("Adding duplicate item");
                }
                Insert(~searchIndex, childNode);
            }
        }

        public void AddRange(params BaseNode[] baseNodes)
        {
            AddRange((IEnumerable<BaseNode>) baseNodes);
        }

        public new int BinarySearch(BaseNode item)
        {
            return BinarySearch(item, item);
        }
    }
}