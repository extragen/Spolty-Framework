using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;
using Spolty.Framework.EnumeratorProviders;
using Spolty.Framework.ExpressionMakers.Linq;
using Spolty.Framework.Helpers;

namespace Spolty.Framework.ExpressionMakers.Factories
{
	/* In constructor
	 * 
	 * Type type = currentContext.GetType();
	 * IEnumerable<MethodInfo> methodInfos = type.GetMethods(ReflectionHelper.PublicMemberFlag).Where(pi => pi.Name.StartsWith("get_"));
	 * foreach (var methodInfo in methodInfos)
	 * {
	 * 	Func<object, object> method = ReflectionHelper.MakeDelegateMethod(type, methodInfo);
	 * 	_methods.Add(ReflectionHelper.GetGenericType(methodInfo.ReturnType).Name, method);
	 * }
	 * 
	 * In GetTable method
	 * 
	 * (IQueryable) _methods[entityType.Name](CurrentContext);
	 * 
	 */

	public class EntityFrameworkExpressionMakerFactory : IExpressionMakerFactory
	{
		private readonly Dictionary<string, Func<object, object>> _methods = new Dictionary<string, Func<object, object>>();
		
		public EntityFrameworkExpressionMakerFactory(object currentContext)
		{
			Checker.CheckArgumentNull(currentContext as ObjectContext, "currentContext");
			CurrentContext = currentContext;
			Store = new Dictionary<string, object>(0);

			Type type = currentContext.GetType();
			IEnumerable<MethodInfo> methodInfos = type.GetMethods(ReflectionHelper.PublicMemberFlag).Where(pi => pi.Name.StartsWith("get_"));
			foreach (var methodInfo in methodInfos)
			{
				Func<object, object> method = ReflectionHelper.MakeDelegateMethod(type, methodInfo);
				_methods.Add(ReflectionHelper.GetGenericType(methodInfo.ReturnType).Name, method);
			}
		}

		#region Implementation of IExpressionMakerFactory

		public Dictionary<string, object> Store { get; private set; }

		#endregion

		#region IExpressionMakerFactory Members

		public object CurrentContext { get; private set; }

		public IJoinExpressionMaker CreateJoinExpressionMaker()
		{
			return new JoinExpressionMaker(this);
		}

		public IOrderingExpressionMaker CreateOrderingExpressionMaker()
		{
			return new OrderingExpressionMaker(this);
		}

		public IConditionExpressionMaker CreateConditionExpressionMaker()
		{
			return new ConditionExpressionMaker(this);
		}

		public ISimpleExpressionMaker CreateSimpleExpressionMaker()
		{
			return new ExpressionMaker(this);
		}

		public IQueryable GetTable(Type entityType)
		{
			var context = CurrentContext;
			PropertyInfo propertyInfo =
				context.GetType().GetProperties(ReflectionHelper.PublicMemberFlag).FirstOrDefault(
					pi => ReflectionHelper.GetGenericType(pi.PropertyType) == entityType);
			return (IQueryable) propertyInfo.GetValue(context, null);
		}

		public IEnumeratorProvider CreateEnumeratorProvider(Type entityType, IQueryProvider provider, Expression expression)
		{
			return new DefaultEnumeratorProvider(entityType, provider, expression);
		}

		#endregion
	}
}