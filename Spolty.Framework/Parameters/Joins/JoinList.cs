using System;
using System.Collections.Generic;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Conditionals;

namespace Spolty.Framework.Parameters.Joins
{
    [Serializable]
    internal class JoinList : List<JoinItem>
    {
        #region Constructors

        public JoinList()
        {
        }

        public JoinList(IEnumerable<JoinItem> joins)
        {
            AddJoins(joins);
        }

        public JoinList(params JoinItem[] joins)
        {
            AddJoins(joins);
        }

        #endregion Constsructors

        #region Public Methods

        public void AddJoins(IEnumerable<JoinItem> joins)
        {
            AddRange(joins);
        }

        public void AddJoins(params JoinItem[] joins)
        {
            AddRange(joins);
        }

        public int RemoveDuplicates()
        {
            return ListWrapperHelper.RemoveDuplicates(this);
        }

        public static implicit operator JoinNode(JoinList current)
        {
            JoinNode parentNode = null;
            if (current.Count > 0)
            {
                current.RemoveDuplicates();
                parentNode = new JoinNode(current[0].Left.EntityType);
                JoinNode joinNode = new JoinNode(current[0].Right.EntityType, current[0].Left.Fields, current[0].Right.Fields, current[0].JoinType);
                parentNode.ChildNodes.Add(joinNode);
                for (int index = 1; index < current.Count; index++)
                {
                    Type leftBizObjType = current[index].Left.EntityType;
                    if (parentNode.EntityType == leftBizObjType)
                    {
                        joinNode = parentNode;
                    }
                    else if (joinNode.EntityType != leftBizObjType)
                    {
                        joinNode = (JoinNode) parentNode.Find(leftBizObjType);
                        if (joinNode == null)
                        {
                            throw new SpoltyException("joinNode is not found");
                        }
                    }
                    joinNode.ChildNodes.Add(new JoinNode(current[index].Right.EntityType, current[index].Left.Fields, current[index].Right.Fields, current[index].JoinType));
                }
            }
            return parentNode;
        }

        #endregion Public Methods
    }
}