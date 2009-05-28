using Spolty.Framework.Parameters.BaseNode;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.Parameters.Joins
{
    public class JoinNodeList : BaseNodeList
    {
        public JoinNodeList(JoinNode parentNode)
            : base(parentNode)
        {
        }

        public override void Add(BaseNode.BaseNode childNode)
        {
            var joinChildNode = (JoinNode) childNode;
            var parentNode = (JoinNode) ParentNode;
            if (parentNode.JoinParentType == JoinType.LeftJoin)
            {
                joinChildNode.JoinParentType = JoinType.LeftJoin;
            }
            base.Add(joinChildNode);
        }
    }
}