using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Designers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;
using Spolty.Framework.Parameters;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Joins.Enums;
using Spolty.Framework.Parameters.Orderings;
using Spolty.Framework.Parsers;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class JoinExpressionMaker : IJoinExpressionMaker
    {
        private readonly IExpressionMakerFactory _factory;

        public IExpressionMakerFactory Factory
        {
            get { return _factory; }
        }

        public JoinExpressionMaker(IExpressionMakerFactory factory) 
        {
            _factory = factory;
        }

        #region Gets Selector Keys Methods

        private void GetSelectorKeys(Type outerType, Type innerType,
                                     out LambdaExpression outerKeySelector,
                                     out LambdaExpression innerKeySelector)
        {
            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            ParameterExpression innerParam = ExpressionHelper.CreateOrGetParameterExpression(innerType, innerType.Name, Factory.Store);

            object context = Factory.CurrentContext;

            if (!(context is DataContext))
            {
                throw new SpoltyException("context not inherited from DataContext");
            }

            MetaAssociation association =
                ((DataContext) context).Mapping.GetTable(outerType).RowType.Associations.FirstOrDefault(
                    searchAss => searchAss.OtherType.Type == innerType);
            if (association == null)
            {
                throw new SpoltyException("Property not found");
            }

            MemberInfo mi = association.ThisMember.Member;

            Type memberType = ReflectionHelper.GetMemberType(mi);
            if (ExpressionHelper.IEnumerableType.IsAssignableFrom(memberType))
            {
                outerKeySelector = Expression.Lambda(outerParam, outerParam);
                Expression propertyExpreesion = ExpressionHelper.CreateMemberExpression(innerType, outerParam.Name, innerParam);
                innerKeySelector = Expression.Lambda(propertyExpreesion, innerParam);
            }
            else
            {
                MemberExpression memberExpression;

                if (mi is PropertyInfo)
                {
                    memberExpression = Expression.Property(outerParam, mi as PropertyInfo);
                }
                else if (mi is FieldInfo)
                {
                    memberExpression = Expression.Field(outerParam, mi as FieldInfo);
                }
                else
                {
                    throw new SpoltyException("Not supported member type");
                }

                outerKeySelector = Expression.Lambda(memberExpression, outerParam);
                innerKeySelector = Expression.Lambda(innerParam, innerParam);
            }
        }

        private void GetSelectorKeys(Type outerType, Type innerType, string outerProperty,
                                     out LambdaExpression outerKeySelector,
                                     out LambdaExpression innerKeySelector)
        {
            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            ParameterExpression innerParam = ExpressionHelper.CreateOrGetParameterExpression(innerType, innerType.Name, Factory.Store);

            PropertyInfo pi = outerType.GetProperty(innerType.Name);
            if (pi == null)
            {
                throw new SpoltyException(String.Format("Property:{0} not found", innerType.Name));
            }

            if (ExpressionHelper.IEnumerableType.IsAssignableFrom(pi.PropertyType))
            {
                outerKeySelector = Expression.Lambda(outerParam, outerParam);
                Expression propertyExpreesion = ExpressionHelper.CreateMemberExpression(innerType, outerProperty, innerParam);
                innerKeySelector = Expression.Lambda(propertyExpreesion, innerParam);
            }
            else
            {
                Expression propertyExpreesion = ExpressionHelper.CreateMemberExpression(outerType, innerType.Name, outerParam);
                outerKeySelector = Expression.Lambda(propertyExpreesion, outerParam);
                innerKeySelector = Expression.Lambda(innerParam, innerParam);
            }
        }

        #endregion

        #region Left Join Makers Method

        internal Expression MakeLeftJoin(Expression outerSourceExpression, JoinNode rootNode, int startIndex)
        {
            Type outerType = ExpressionHelper.GetGenericType(outerSourceExpression);

            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            var dynamicProperties = new Dictionary<DynamicProperty, Expression>(rootNode.ChildNodes.Count - startIndex + 1);
            var outerProperty = new DynamicProperty(MethodName.MainProperty, outerType);
            dynamicProperties.Add(outerProperty, outerParam);
            
            for (int index = startIndex; index < rootNode.ChildNodes.Count; index++)
            {
                var childNode = (JoinNode) rootNode.ChildNodes[index];
                Expression innerMemberEx = Expression.PropertyOrField(outerParam, childNode.ParentPropertyName);
                if (childNode.Conditions.Count > 0)
                {
                    innerMemberEx = Factory.CreateConditionExpressionMaker().Make(childNode.Conditions, innerMemberEx);
                }

                if (childNode.ChildNodes.Count > 0)
                {
                    innerMemberEx = MakeLeftJoin(innerMemberEx, childNode, 0);
                }

                var innerProperty = new DynamicProperty(childNode.ParentPropertyName, innerMemberEx.Type);
                dynamicProperties.Add(innerProperty, innerMemberEx);
            }
            
            Type dynamicClassType = DynamicExpression.CreateClass(dynamicProperties.Keys);

            int keyIndex = 0;
            var bindings = new MemberBinding[dynamicProperties.Keys.Count];
            foreach (var key in dynamicProperties.Keys)
            {
                PropertyInfo dynamicPropertyInfo = dynamicClassType.GetProperty(key.Name);
                bindings[keyIndex] = Expression.Bind(dynamicPropertyInfo, dynamicProperties[key]);
                keyIndex++;
            }

            Expression newExpression = Expression.MemberInit(Expression.New(dynamicClassType), bindings);
            LambdaExpression resultSelector = Expression.Lambda(newExpression, outerParam);

            Expression selectNewQueryableExpression = null;
            if (outerSourceExpression.Type.GetInterface(ExpressionHelper.IQueryableType.Name) != null)
            {
                selectNewQueryableExpression = ExpressionHelper.CallQueryableMethod(MethodName.Select, new[] {outerType, resultSelector.Body.Type}, outerSourceExpression, Expression.Quote(resultSelector));
            }
            else if (outerSourceExpression.Type.GetInterface(ExpressionHelper.IEnumerableType.Name) != null)
            {
                selectNewQueryableExpression = ExpressionHelper.CallEnumerableMethod(MethodName.Select, new[] {outerType, resultSelector.Body.Type}, outerSourceExpression, resultSelector);
            }
           
            if (selectNewQueryableExpression == null)
            {
                throw new SpoltyException(String.Format("Can not create left join between parent {0} and children.", rootNode.EntityType.Name));
            }

            return selectNewQueryableExpression;
        }

        #endregion

        #region Inner Join Makers Method

        internal Expression MakeInnerJoin(Expression outerSourceExpression, Expression innerSourceExpression,
                                          Type unitingType, Type resultSelectorType)
        {
            Type outerType = ExpressionHelper.GetGenericType(outerSourceExpression);
            Type innerType = ExpressionHelper.GetGenericType(innerSourceExpression);

            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            ParameterExpression innerParam = ExpressionHelper.CreateOrGetParameterExpression(innerType, innerType.Name, Factory.Store);

            LambdaExpression outerKeySelector;
            LambdaExpression innerKeySelector;

            GetSelectorKeys(outerType, innerType, out outerKeySelector, out innerKeySelector);

            LambdaExpression resultSelector = Expression.Lambda(Expression.Constant(resultSelectorType.Name), outerParam,
                                                                innerParam);

            Expression joinExpression = CallJoinMethod(outerSourceExpression, innerSourceExpression, outerKeySelector,
                                                       innerKeySelector, resultSelector);

            return joinExpression;
        }

        private Expression MakeInnerJoin(Expression outerSourceExpression, Expression innerSourceExpression,
                                         JoinNode childNode, Type resultSelectorType)
        {
            Type outerType = ExpressionHelper.GetGenericType(outerSourceExpression);
            Type innerType = ExpressionHelper.GetGenericType(innerSourceExpression);

            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            ParameterExpression innerParam = ExpressionHelper.CreateOrGetParameterExpression(innerType, innerType.Name, Factory.Store);

            LambdaExpression outerKeySelector;
            LambdaExpression innerKeySelector;

            if (childNode.IsTypeNameEqualAssociatedPropertyName)
            {
                GetSelectorKeys(outerType, innerType, out outerKeySelector,
                                out innerKeySelector);
            }
            else
            {
                GetSelectorKeys(outerType, innerType, childNode.AssociatedPropertyName, out outerKeySelector,
                                out innerKeySelector);
            }

            LambdaExpression resultSelector =
                Expression.Lambda(resultSelectorType == outerType ? outerParam : innerParam, outerParam, innerParam);

            if (childNode.Conditions.Count > 0)
            {
                innerSourceExpression = Factory.CreateConditionExpressionMaker().Make(childNode.Conditions,
                                                                                      innerSourceExpression);
            }

            Expression joinExpression = CallJoinMethod(outerSourceExpression, innerSourceExpression, outerKeySelector,
                                                       innerKeySelector, resultSelector);

            return joinExpression;
        }

        private Expression MakeInnerJoin(Expression outerExpression, Expression innerExpression,
                                         string[] leftFieldsNames, string[] rightFieldsNames,
                                         ConditionList conditions)
        {
            Checker.CheckArgumentNull(outerExpression, "outerExpression");
            Checker.CheckArgumentNull(innerExpression, "innerExpression");
            Checker.CheckArgumentNull(leftFieldsNames, "leftFieldsNames");
            Checker.CheckArgumentNull(outerExpression, "outerExpression");

            int fieldsCount = leftFieldsNames.Length;
            if (fieldsCount != rightFieldsNames.Length)
            {
                throw new SpoltyException("Number of fields mismatch");
            }

            Type outerType = ExpressionHelper.GetGenericType(outerExpression);
            ParameterExpression outerParam = ExpressionHelper.CreateOrGetParameterExpression(outerType, outerType.Name, Factory.Store);
            Type innerType = ExpressionHelper.GetGenericType(innerExpression);
            ParameterExpression innerParam = ExpressionHelper.CreateOrGetParameterExpression(innerType, innerType.Name, Factory.Store);

            var clw = new ConditionList();

            for (int index = 1; index < fieldsCount; index++)
            {
                var fieldCondition =
                    new FieldCondition(leftFieldsNames[index],
                                       ConditionOperator.EqualTo, rightFieldsNames[index], outerType, innerType);
                clw.Add(fieldCondition);
            }

            PropertyInfo outerPropertyInfo = outerType.GetProperty(leftFieldsNames[0]);
            if (outerPropertyInfo == null)
            {
                throw new SpoltyException("Property not found");
            }
            PropertyInfo innerPropertyInfo = innerType.GetProperty(rightFieldsNames[0]);
            if (innerPropertyInfo == null)
            {
                throw new SpoltyException("Property not found");
            }

            Expression outerPropertyExpression = Expression.Property(outerParam, outerPropertyInfo);
            Expression innerPropertyExpression = Expression.Property(innerParam, innerPropertyInfo);

            Type type = outerPropertyInfo.PropertyType;
            innerPropertyExpression =
                ExpressionHelper.ConvertOrCastInnerPropertyExpression(type, innerPropertyInfo, innerPropertyExpression);

            LambdaExpression outerKeySelector = Expression.Lambda(outerPropertyExpression, outerParam);
            LambdaExpression innerKeySelector = Expression.Lambda(innerPropertyExpression, innerParam);
            LambdaExpression resultSelector = Expression.Lambda(outerParam, outerParam, innerParam);

            if (conditions != null && conditions.Count > 0)
            {
                clw.AddConditions(conditions);
            }

            Expression resultJoinExpression = CallJoinMethod(outerExpression, innerExpression, outerKeySelector,
                                                             innerKeySelector,
                                                             resultSelector);
            resultJoinExpression = Factory.CreateConditionExpressionMaker().Make(clw, resultJoinExpression);
            return resultJoinExpression;
        }

        private static Expression CallJoinMethod(Expression outer, Expression inner,
                                                 LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
                                                 LambdaExpression resultSelector)
        {
            var typeArguments = new[]
                                    {
                                        ExpressionHelper.GetGenericType(outer), ExpressionHelper.GetGenericType(inner),
                                        outerKeySelector.Body.Type, resultSelector.Body.Type
                                    };

            return ExpressionHelper.CallQueryableMethod(MethodName.Join, typeArguments, outer, inner, Expression.Quote(outerKeySelector),
                                       Expression.Quote(innerKeySelector), Expression.Quote(resultSelector));
        }

        #endregion

        #region IJoinExpressionMaker Members

        public Expression Make(Expression rootExpression, JoinNode rootNode, params IParameterMarker[] parameters)
        {
            Checker.CheckArgumentNull(rootExpression, "rootExpression");
            Checker.CheckArgumentNull(rootNode, "rootNode");

            ConditionList conditions;
            OrderingList orderings;
            ParametersParser.Parse(out conditions, out orderings, parameters);
            Expression newExpression = rootExpression;
            int index = 0;
            foreach (JoinNode childNode in rootNode.ChildNodes)
            {
                if (childNode.JoinWithParentBy == JoinType.InnerJoin)
                {
                    index++;
                    if (childNode.ChildNodes.Count > 0)
                    {
                        var queryDesinger = new QueryDesigner(Factory.CurrentContext, childNode.EntityType);
                        queryDesinger = queryDesinger.Where(conditions).OrderBy(orderings);

                        Expression outerExpression = queryDesinger.Expression;
                        Expression childrenExpression = Make(outerExpression, childNode, conditions, orderings);
                        newExpression = MakeInnerJoin(newExpression, childrenExpression, childNode);
                    }
                    else
                    {
                        var queryDesinger = new QueryDesigner(Factory.CurrentContext, childNode.EntityType);
                        queryDesinger = queryDesinger.Where(childNode.Conditions).OrderBy(orderings);

                        newExpression = MakeInnerJoin(newExpression, queryDesinger.Expression, childNode);
                    }
                }
                else
                {
                    break;
                }
            }

            if (index < rootNode.ChildNodes.Count)
            {
                newExpression = MakeLeftJoin(newExpression, rootNode, index); 
            }

            return newExpression;
        }

        #endregion

        internal Expression MakeInnerJoin(Expression outerExpression, Expression innerExpression, JoinNode childNode)
        {
            Checker.CheckArgumentNull(outerExpression, "outerExpression");
            Checker.CheckArgumentNull(innerExpression, "innerExpression");
            Checker.CheckArgumentNull(childNode, "childNode");

            Type resultType = ExpressionHelper.GetGenericType(outerExpression);
            if (childNode.ParentFieldsNames.Count > 0 && childNode.CurrentFieldsNames.Count > 0)
            {
                var leftFields = new string[childNode.ParentFieldsNames.Count];
                var rightFields = new string[childNode.CurrentFieldsNames.Count];

                childNode.ParentFieldsNames.CopyTo(leftFields, 0);
                childNode.CurrentFieldsNames.CopyTo(rightFields, 0);

                outerExpression = MakeInnerJoin(outerExpression, innerExpression,
                                                leftFields, rightFields,
                                                childNode.Conditions);
            }
            else
            {
                outerExpression = MakeInnerJoin(outerExpression, innerExpression,
                                                childNode, resultType);
            }
            return outerExpression;
        }
    }
}