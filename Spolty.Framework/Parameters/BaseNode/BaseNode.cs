using System;
using System.Collections.Generic;

namespace Spolty.Framework.Parameters.BaseNode
{
    public abstract class BaseNode : IComparer<BaseNode> 
    {
        private Type _entityType;

        internal BaseNode _parentNode = null;

        protected BaseNodeList _childNodes;
        private int _level;

        public BaseNode ParentNode
        {
            get { return _parentNode; }
        }

        public BaseNodeList ChildNodes
        {
            get { return _childNodes; }
        }

        public Type EntityType
        {
            get { return _entityType; }
            internal set { _entityType = value; }
        }

        public int Level
        {
            get { return _level; }
            internal set { _level = value; }
        }

        protected BaseNode(Type entityType)
        {
            _entityType = entityType;
        }

        public BaseNode Find(Type entityType)
        {
            BaseNode joinNode = GetRootNode();
            return joinNode.FindInChildren(entityType, true);
        }

        public BaseNode GetRootNode()
        {
            BaseNode parentNode = this;
            while (parentNode.ParentNode != null)
            {
                parentNode = parentNode.ParentNode;
            }
            return parentNode;
        }

        public BaseNode FindInChildren(Type type)
        {
            return FindInChildren(type, false);
        }

        public BaseNode FindInChildren(Type type, bool recursive)
        {
            if (!recursive)
            {
                foreach (BaseNode childNode in _childNodes)
                {
                    if (childNode.EntityType == type)
                    {
                        return childNode;
                    }
                }
            }
            else
            {
                return FindInChildren((IEnumerable<BaseNode>) _childNodes, type);
            }
            return null;
        }

        public void AddChildren(params BaseNode[] children)
        {
            AddChildren((IEnumerable<BaseNode>) children);
        }

        public void AddChildren(IEnumerable<BaseNode> chidren)
        {
            ChildNodes.AddRange(chidren);
        }

        private BaseNode FindInChildren(IEnumerable<BaseNode> childNodes, Type bizObjectType)
        {
            foreach (BaseNode childNode in childNodes)
            {
                if (childNode.EntityType == bizObjectType)
                {
                    return childNode;
                }
                
                BaseNode node = FindInChildren((IEnumerable<BaseNode>) childNode.ChildNodes, bizObjectType);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
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
            BaseNode val2 = obj as BaseNode;
            if (val2 != null)
            {
                if (GetHashCode() == val2.GetHashCode())
                {
                    return _entityType == val2._entityType;
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
            int result = _entityType.GetHashCode();
            return result;
        }
		
        #region IComparer<JoinNode> Members

        ///<summary>
        ///Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        ///</summary>
        ///<returns>
        ///Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
        ///</returns>
        ///<param name="y">The second object to compare.</param>
        ///<param name="x">The first object to compare.</param>
        public int Compare(BaseNode x, BaseNode y)
        {
            return x.EntityType.GetHashCode() - y.EntityType.GetHashCode();
        }

        #endregion
    }
}