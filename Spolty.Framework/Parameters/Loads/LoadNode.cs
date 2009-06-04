using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters.BaseNode;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.Parameters.Loads
{
    internal class LoadNode : BaseNode.BaseNode
    {
        public const JoinType JoinParentType = JoinType.LeftOuterJoin;
        private readonly string _propertyName;
        private bool _including = true;

        #region Properties

        public bool Including
        {
            get { return _including; }
            set { _including = value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        #endregion Properties

        #region Constructors

        public LoadNode(Type bizObjectType)
            : base(bizObjectType)
        {
            _childNodes = new BaseNodeList(this);
            _propertyName = bizObjectType.Name;
        }

        public LoadNode(Type bizObjectType, bool including) : this(bizObjectType)
        {
            Including = including;
        }


        public LoadNode(Item item) : base(item.EntityType)
        {
            _propertyName = item.PropertyName;
            _childNodes = new BaseNodeList(this);
        }

        public LoadNode(Item item, bool including) : this(item)
        {
            _including = including;
        }

        #endregion Constructors

        #region Public Methods

        public void AddIncludingTypes(IEnumerable<Type> bizObjectTypes)
        {
            if (ChildNodes.Count == 0)
            {
                foreach (Type bizObjectType in bizObjectTypes)
                {
                    ChildNodes.Add(new LoadNode(bizObjectType, true));
                }
            }
            foreach (Type bizObjectType in bizObjectTypes)
            {
                var child = (LoadNode) FindInChildren(bizObjectType);
                if (child == null)
                {
                    child = new LoadNode(bizObjectType, true);
                    ChildNodes.Add(child);
                }
                else
                {
                    child.Including = true;
                }
            }
        }

        public void AddIncludingTypes(params Type[] bizObjectTypes)
        {
            AddIncludingTypes((IEnumerable<Type>) bizObjectTypes);
        }

        public void AddIncludingItems(IEnumerable<Item> items)
        {
            if (ChildNodes.Count == 0)
            {
                foreach (Item item in items)
                {
                    ChildNodes.Add(new LoadNode(item));
                }
            }
            foreach (Item item in items)
            {
                var child = (LoadNode) FindInChildren(item.EntityType);
                if (child == null)
                {
                    child = new LoadNode(item);
                    ChildNodes.Add(child);
                }
                else
                {
                    child.Including = true;
                }
            }
        }

        public void AddIncludingItems(params Item[] items)
        {
            AddIncludingItems((IEnumerable<Item>) items);
        }

        #endregion Public Methods
    }
}