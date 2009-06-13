using System;
using System.Collections.Generic;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Orderings;

namespace Spolty.Framework.Parsers
{
    public static class ParametersParser
    {
        internal static void Parse(out ConditionList conditions,
                                   out OrderingList orderings,
                                   params IParameterMarker[] queryDesignerParameters)
        {
            conditions = new ConditionList();
            orderings = new OrderingList();

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
                    else
                    {
                        throw new NotSupportedException("parameter");
                    }
                }
                conditions.RemoveDuplicates();
                orderings.RemoveDuplicates();
            }
        }
    }
}