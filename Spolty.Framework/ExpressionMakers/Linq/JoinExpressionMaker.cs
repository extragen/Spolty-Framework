using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;
using Spolty.Framework.Parameters.Conditionals;
using Spolty.Framework.Parameters.Conditionals.Enums;
using Spolty.Framework.Parameters.Joins;
using Spolty.Framework.Parameters.Joins.Enums;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class JoinExpressionMaker : ExpressionMaker, IJoinExpressionMaker
    {
        private const string GroupJoin = "GroupJoin";

        public JoinExpressionMaker(IExpressionMakerFactory factory) : base(factory)
        {
        }

        #region Group Join Expresion Maker

        protected Expression MakeGroupJoinExpression(Expression outerSourceExpression,
                                                     Expression innerSourceExpression, Type unitingType, JoinNode childNode)
        {
            Type outerType = GetTemplateType(outerSourceExpression);
            Type innerType = GetTemplateType(innerSourceExpression);

            LambdaExpression outerKeySelector;
            LambdaExpression innerKeySelector;

            if (childNode.IsTypeNameEqualPropertyName)
            {
                GetOuterAndInnerKeySelectors(outerType, innerType, out outerKeySelector,
                                             out innerKeySelector);
            }
            else
            {
                GetOuterAndInnerKeySelectors(outerType, innerType, childNode.PropertyName, out outerKeySelector,
                                             out innerKeySelector);
            }
            Type innerParam1Type = GenericIEnumerableType.MakeGenericType(new[] {innerType});

            ParameterExpression innerParam1 =
                GetParameterExpression(GenericIEnumerableType.MakeGenericType(new[] {innerType}), innerType.Name);
            ParameterExpression innerParam2 = GetParameterExpression(outerType, outerType.Name);

            var innerProperty = new DynamicProperty(innerType.Name, innerParam1Type);
            var outerProperty = new DynamicProperty(outerType.Name, outerType);

            Type dynamicClassType = DynamicExpression.CreateClass(innerProperty, outerProperty);

            var bindings = new MemberBinding[2];
            bindings[0] = Expression.Bind(dynamicClassType.GetProperty(innerType.Name), innerParam1);
            bindings[1] = Expression.Bind(dynamicClassType.GetProperty(outerType.Name), innerParam2);

            Expression newExpression = Expression.MemberInit(Expression.New(dynamicClassType), bindings);
            LambdaExpression resultSelector = Expression.Lambda(newExpression, innerParam2, innerParam1);

            var typeArguments = new[] {outerType, innerType, outerKeySelector.Body.Type, resultSelector.Body.Type};

            Expression groupJoinExpression = CallQueryableMethod(GroupJoin, typeArguments, outerSourceExpression,
                                                                 innerSourceExpression,
                                                                 Expression.Quote(outerKeySelector),
                                                                 Expression.Quote(innerKeySelector),
                                                                 Expression.Quote(resultSelector));
            return groupJoinExpression;
        }

        #endregion

        #region Gets Outer / Inner Key Selectors Method

        private void GetOuterAndInnerKeySelectors(Type outerType, Type innerType,
                                                  out LambdaExpression outerKeySelector,
                                                  out LambdaExpression innerKeySelector)
        {
            ParameterExpression outerParam = GetParameterExpression(outerType, outerType.Name);
            ParameterExpression innerParam = GetParameterExpression(innerType, innerType.Name);

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
            if (IEnumerableType.IsAssignableFrom(memberType))
            {
                outerKeySelector = Expression.Lambda(outerParam, outerParam);
                Expression propertyExpreesion = GetPropertyExpression(innerType, outerParam.Name, innerParam);
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

        private static void GetOuterAndInnerKeySelectors(Type outerType, Type innerType, string outerProperty,
                                                         out LambdaExpression outerKeySelector,
                                                         out LambdaExpression innerKeySelector)
        {
            ParameterExpression outerParam = GetParameterExpression(outerType, outerType.Name);
            ParameterExpression innerParam = GetParameterExpression(innerType, innerType.Name);

            PropertyInfo pi = outerType.GetProperty(innerType.Name);
            if (pi == null)
            {
                throw new SpoltyException(String.Format("Property:{0} not found", innerType.Name));
            }

            if (IEnumerableType.IsAssignableFrom(pi.PropertyType))
            {
                outerKeySelector = Expression.Lambda(outerParam, outerParam);
                Expression propertyExpreesion = GetPropertyExpression(innerType, outerProperty, innerParam);
                innerKeySelector = Expression.Lambda(propertyExpreesion, innerParam);
            }
            else
            {
                Expression propertyExpreesion = GetPropertyExpression(outerType, innerType.Name, outerParam);
                outerKeySelector = Expression.Lambda(propertyExpreesion, outerParam);
                innerKeySelector = Expression.Lambda(innerParam, innerParam);
            }
        }

        #endregion

        #region Left Join Makers Method

        internal Expression MakeLeftJoin(Expression outerSourceExpression,
                                         Expression innerSourceExpression, Type unitingTyped,
                                         Type resultSelectorType)
        {
            return
                MakeLeftJoin(outerSourceExpression, innerSourceExpression, unitingTyped, resultSelectorType,
                             null);
        }

        protected Expression MakeLeftJoin(Expression outerSourceExpression,
                                  Expression innerSourceExpression, Type unitingTyped,
                                  Type resultSelectorType, JoinNode childNode)
        {
            Expression groupJoinExpression =
                MakeGroupJoinExpression(outerSourceExpression, innerSourceExpression, unitingTyped, childNode);

            Type outerType = GetTemplateType(outerSourceExpression);
            Type innerType = GetTemplateType(innerSourceExpression);

            Type groupJoinBodyType = GetTemplateType(groupJoinExpression);
            ParameterExpression dynParam1 = GetParameterExpression(groupJoinBodyType, groupJoinBodyType.Name);

            PropertyInfo innerPropertyInfo = groupJoinBodyType.GetProperty(innerType.Name);
            ParameterExpression joinParam = GetParameterExpression(innerType, innerType.Name);

            MemberExpression innerMemberEx = Expression.Property(dynParam1, innerPropertyInfo);
            MemberExpression outerMemberEx = Expression.Property(dynParam1, outerType.Name);

            Expression tempJoinExpression = CallEnumerableMethod(MethodName.DefaultIfEmpty, new[] { innerType }, innerMemberEx);

            var outerProperty = new DynamicProperty(outerType.Name, outerType);
            var innerProperty = new DynamicProperty(innerType.Name, innerType);

            Type dynamicClassType2 = DynamicExpression.CreateClass(outerProperty, innerProperty);

            var bindings = new MemberBinding[2];
            bindings[0] = Expression.Bind(dynamicClassType2.GetProperty(outerType.Name), outerMemberEx);
            bindings[1] = Expression.Bind(dynamicClassType2.GetProperty(innerType.Name), joinParam);

            Expression newExpression = Expression.MemberInit(Expression.New(dynamicClassType2), bindings);
            LambdaExpression resultSelector = Expression.Lambda(newExpression, joinParam);

            tempJoinExpression = CallEnumerableMethod(MethodName.Select, new[] { innerType, resultSelector.Body.Type },
                                                      tempJoinExpression, resultSelector);

            LambdaExpression arguments = Expression.Lambda(tempJoinExpression, dynParam1);
            tempJoinExpression = CallQueryableMethod(MethodName.SelectMany, new[] { groupJoinBodyType, dynamicClassType2 },
                                                     groupJoinExpression, arguments);

            ParameterExpression dynParam2 = GetParameterExpression(dynamicClassType2, dynamicClassType2.Name);

            resultSelector = Expression.Lambda(outerMemberEx, dynParam2);

            Expression leftJoinExpression = CallQueryableMethod(MethodName.Select,
                                                                new[]
                                                                    {
                                                                        GetTemplateType(tempJoinExpression),
                                                                        resultSelector.Body.Type
                                                                    }, tempJoinExpression,
                                                                resultSelector);

            if (childNode.Conditions != null && childNode.Conditions.Count > 0)
            {
                leftJoinExpression = Factory.CreateConditionExpressionMaker().Make(childNode.Conditions, leftJoinExpression);
            }

            return leftJoinExpression;
        }

        #endregion

        #region Inner Join Makers Method

        internal Expression MakeInnerJoin(Expression outerSourceExpression, Expression innerSourceExpression,
                                          Type unitingType, Type resultSelectorType)
        {
            Type outerType = GetTemplateType(outerSourceExpression);
            Type innerType = GetTemplateType(innerSourceExpression);

            ParameterExpression outerParam = GetParameterExpression(outerType, outerType.Name);
            ParameterExpression innerParam = GetParameterExpression(innerType, innerType.Name);

            LambdaExpression outerKeySelector;
            LambdaExpression innerKeySelector;

            GetOuterAndInnerKeySelectors(outerType, innerType, out outerKeySelector, out innerKeySelector);

            LambdaExpression resultSelector = Expression.Lambda(Expression.Constant(resultSelectorType.Name), outerParam,
                                                                innerParam);

            Expression joinExpression = CallJoinMethod(outerSourceExpression, innerSourceExpression, outerKeySelector,
                                                       innerKeySelector, resultSelector);

            return joinExpression;
        }

        private Expression MakeInnerJoin(Expression outerSourceExpression, Expression innerSourceExpression,
                                         JoinNode childNode, Type resultSelectorType)
        {
            Type outerType = GetTemplateType(outerSourceExpression);
            Type innerType = GetTemplateType(innerSourceExpression);

            ParameterExpression outerParam = GetParameterExpression(outerType, outerType.Name);
            ParameterExpression innerParam = GetParameterExpression(innerType, innerType.Name);

            LambdaExpression outerKeySelector;
            LambdaExpression innerKeySelector;

            if (childNode.IsTypeNameEqualPropertyName)
            {
                GetOuterAndInnerKeySelectors(outerType, innerType, out outerKeySelector,
                                             out innerKeySelector);
            }
            else
            {
                GetOuterAndInnerKeySelectors(outerType, innerType, childNode.PropertyName, out outerKeySelector,
                                             out innerKeySelector);
            }

            LambdaExpression resultSelector =
                Expression.Lambda(resultSelectorType == outerType ? outerParam : innerParam, outerParam, innerParam);

            if (childNode.Conditions.Count > 0)
            {
                innerSourceExpression = Factory.CreateConditionExpressionMaker().Make(childNode.Conditions, innerSourceExpression);
            }

            Expression joinExpression = CallJoinMethod(outerSourceExpression, innerSourceExpression, outerKeySelector,
                                                       innerKeySelector, resultSelector);

            return joinExpression;
        }

        private Expression MakeInnerJoin(Expression outerExpression, Expression innerExpression,
                                         string[] leftFieldsNames, string[] rightFieldsNames,
                                         ConditionList conditions)
        {
            if (outerExpression == null)
            {
                throw new ArgumentNullException("outerExpression");
            }
            if (innerExpression == null)
            {
                throw new ArgumentNullException("innerExpression");
            }
            if (leftFieldsNames == null)
            {
                throw new ArgumentNullException("leftFieldsNames");
            }
            if (rightFieldsNames == null)
            {
                throw new ArgumentNullException("rightFieldsNames");
            }

            int fieldsCount = leftFieldsNames.Length;
            if (fieldsCount != rightFieldsNames.Length)
            {
                throw new SpoltyException("Number of fields mismatch");
            }

            Type outerType = GetTemplateType(outerExpression);
            ParameterExpression outerParam = GetParameterExpression(outerType, outerType.Name);
            Type innerType = GetTemplateType(innerExpression);
            ParameterExpression innerParam = GetParameterExpression(innerType, innerType.Name);

            var clw = new ConditionList();

            for (int index = 1; index < fieldsCount; index++)
            {
                var fieldCondition =
                    new FieldCondition(leftFieldsNames[index], rightFieldsNames[index],
                                       ConditionOperator.EqualTo, outerType, innerType);
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
                ConvertOrCastInnerPropertyExpression(type, innerPropertyInfo, innerPropertyExpression);

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
                                        GetTemplateType(outer), GetTemplateType(inner),
                                        outerKeySelector.Body.Type, resultSelector.Body.Type
                                    };

            return CallQueryableMethod("Join", typeArguments, outer, inner, Expression.Quote(outerKeySelector),
                                       Expression.Quote(innerKeySelector), Expression.Quote(resultSelector));
        }

        #endregion

        #region IJoinExpressionMaker Members

        public Expression Make(Expression outerExpression, Expression innerExpression,
                               JoinNode childNode, ConditionList conditions)
        {
            Checker.CheckArgumentNull(outerExpression, "outerExpression");
            Checker.CheckArgumentNull(innerExpression, "innerExpression");
            Checker.CheckArgumentNull(childNode, "childNode");
            
            Type unitingType = GetTemplateType(innerExpression);
            Type resultType = GetTemplateType(outerExpression);
            switch (childNode.JoinParentType)
            {
                case JoinType.InnerJoin:

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
                    break;
                case JoinType.LeftJoin:
                    outerExpression = MakeLeftJoin(outerExpression, innerExpression,
                                                   unitingType, resultType,
                                                   childNode);
                    break;
                default:
                    throw new NotSupportedException("JoinType");
            }
            return outerExpression;
        }

        #endregion
    }
}