using System;
using System.Linq.Expressions;
using Spolty.Framework.Checkers;
using Spolty.Framework.Exceptions;
using Spolty.Framework.ExpressionMakers.Factories;
using Spolty.Framework.Helpers;

namespace Spolty.Framework.ExpressionMakers.Linq
{
    internal class ExpressionMaker : ISimpleExpressionMaker
    {
        private readonly IExpressionMakerFactory _factory;

        #region Constructors

        public ExpressionMaker(IExpressionMakerFactory factory)
        {
            _factory = factory;
        }

        #endregion

        #region IExpressionMaker Members

        public IExpressionMakerFactory Factory
        {
            get { return _factory; }
        }

        Expression ISimpleExpressionMaker.MakeExcept(Expression sourceExpression, Expression exceptExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");
            Checker.CheckArgumentNull(exceptExpression, "exceptExpression");

            Type sourceType = ExpressionHelper.GetGenericType(sourceExpression);
            Type exceptType = ExpressionHelper.GetGenericType(exceptExpression);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of sourceExpression mismatch type of exceptExpression");
            }

            return ExpressionHelper.CallQueryableMethod(MethodName.Except, new[] {sourceType}, sourceExpression,
                                                        exceptExpression);
        }

        Expression ISimpleExpressionMaker.MakeSkip(int count, Expression sourceExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.Skip, sourceExpression, Expression.Constant(count));
        }

        Expression ISimpleExpressionMaker.MakeTake(int count, Expression sourceExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.Take, sourceExpression, Expression.Constant(count));
        }

        Expression ISimpleExpressionMaker.MakeUnion(Expression sourceExpression, Expression unionExpression)
        {
            Checker.CheckArgumentNull(sourceExpression, "sourceExpression");
            Checker.CheckArgumentNull(unionExpression, "unionExpression");

            Type sourceType = ExpressionHelper.GetGenericType(sourceExpression);
            Type exceptType = ExpressionHelper.GetGenericType(unionExpression);

            if (sourceType != exceptType)
            {
                throw new SpoltyException("Type of sourceExpression mismatch type of unionExpression");
            }

            return ExpressionHelper.CallQueryableMethod(MethodName.Union, new[] {sourceType}, sourceExpression,
                                                        unionExpression);
        }

        Expression ISimpleExpressionMaker.MakeDistinct(Expression sourceExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.Distinct, sourceExpression, null);
        }

        Expression ISimpleExpressionMaker.MakeAny(Expression sourceExpression, Expression conditionExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.Any, sourceExpression, conditionExpression);
        }

        Expression ISimpleExpressionMaker.MakeCount(Expression sourceExpression, Expression conditionExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.Count, sourceExpression, conditionExpression);
        }

        Expression ISimpleExpressionMaker.MakeFirst(Expression sourceExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.First, sourceExpression, null);
        }

        Expression ISimpleExpressionMaker.MakeFirstOrDefault(Expression sourceExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.FirstOrDefault, sourceExpression, null);
        }

        #endregion

        #region Implementation of IExpressionMaker

        public Expression MakeAll(Expression sourceExpression, Expression conditionExpression)
        {
            return ExpressionHelper.CallMethod(MethodName.All, sourceExpression, conditionExpression);
        }

        #endregion
    }
}