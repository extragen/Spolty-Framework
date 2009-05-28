using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Loads;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parameters.Paging;

namespace Spolty.Framework.Parsers
{
    public static class ParametersParser
    {
        internal static void Parse(out ConditionList conditions,
                                   out OrderingList orderings,
                                   params IParameterMarker[] queryDesignerParameters)
        {
            JoinNode joinNode;
            LoadTree loadTree;
            PagingParameter paging;
            Parse(out conditions, out orderings, out joinNode, out loadTree, out paging, queryDesignerParameters);
        }

        internal static void Parse(out ConditionList conditions,
                                   out OrderingList orderings,
                                   out JoinNode joinNode,
                                   out LoadTree loadTree,
                                   out PagingParameter paging,
                                   params IParameterMarker[] queryDesignerParameters)
        {
            conditions = new ConditionList();
            orderings = new OrderingList();
            joinNode = null;
            loadTree = null;
            paging = null;
            foreach (IParameterMarker parameter in queryDesignerParameters)
            {
                if (parameter != null)
                {
                    if (parameter is ConditionList)
                    {
                        conditions.AddConditions(parameter as IEnumerable<BaseCondition>);
                    }
                    else if (parameter is OrderingList)
                    {
                        orderings.AddOrderings(parameter as OrderingList);
                    }
                    else if (parameter is JoinNode)
                    {
                        joinNode = parameter as JoinNode;
                    }
                    else if (parameter is LoadTree)
                    {
                        loadTree = parameter as LoadTree;
                    }
                    else if (parameter is PagingParameter)
                    {
                        paging = parameter as PagingParameter;
                    }
                    else
                    {
                        throw new NotSupportedException("parameter");
                    }
                }
            }
        }
		
    }
}