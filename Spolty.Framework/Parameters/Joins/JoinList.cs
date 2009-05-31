using System;
using System.Collections.Generic;
using Spolty.Framework.Exceptions;
using Spolty.Framework.Parameters.Conditionals;

namespace Spolty.Framework.Parameters.Joins
{
    [Serializable]
    public class JoinList : List<JoinItem>
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
                parentNode = new JoinNode(current[0].Left.BizObjectType);
                JoinNode joinNode = new JoinNode(current[0].Right.BizObjectType, current[0].JoinType, current[0].Left.Fields, current[0].Right.Fields);
                parentNode.ChildNodes.Add(joinNode);
                for (int index = 1; index < current.Count; index++)
                {
                    Type leftBizObjType = current[index].Left.BizObjectType;
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
                    joinNode.ChildNodes.Add(new JoinNode(current[index].Right.BizObjectType, current[index].JoinType, current[index].Left.Fields, current[index].Right.Fields));
                }
            }
            return parentNode;
        }

        #endregion Public Methods
    }
}