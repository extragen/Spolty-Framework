using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spolty.Framework.Checkers;

namespace Spolty.Framework.Helpers
{
    public static class ReflectionHelper
    {
        public const BindingFlags PublicMemberFlag = BindingFlags.GetField | BindingFlags.GetProperty |
                                                     BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        public const BindingFlags PrivateMemberFlag = BindingFlags.GetField | BindingFlags.GetProperty |
                                                      BindingFlags.DeclaredOnly | BindingFlags.NonPublic |
                                                      BindingFlags.Instance;

        public static MemberInfo GetMemberInfo(Type type, string name)
        {
            Checker.CheckArgumentNull(type, "type");

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            MemberInfo[] res = type.GetMember(name, PublicMemberFlag | PrivateMemberFlag);
            if (res != null && res.Length > 0)
            {
                return res[0];
            }
            return null;
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            Checker.CheckArgumentNull(memberInfo, "memberInfo");

            if (memberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)memberInfo;
                return fi.FieldType;
            }
            if (!(memberInfo is PropertyInfo))
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", memberInfo.GetType().Name));
            }
            PropertyInfo pi = (PropertyInfo) memberInfo;
            return pi.PropertyType;
        }

        public static object GetValue(MemberInfo memberInfo, object source)
        {
            Checker.CheckArgumentNull(memberInfo, "memberInfo");
            Checker.CheckArgumentNull(source, "source");

            if (memberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo) memberInfo;
                return fi.GetValue(source);
            }

            if (memberInfo is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo) memberInfo;
                return pi.GetValue(source, null);
            }
            else
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", memberInfo.GetType().Name));
            }
        }

        public static void SetValue(MemberInfo memberInfo, object source, object value)
        {
            Checker.CheckArgumentNull(memberInfo, "memberInfo");
            Checker.CheckArgumentNull(source, "source");
            Checker.CheckArgumentNull(value, "value");

            if (memberInfo is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)memberInfo;
                fi.SetValue(source, value);
            }
            else if (memberInfo is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)memberInfo;
                pi.SetValue(source, value, null);
            }
            else
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", memberInfo.GetType().Name));
            }
        }

        public static bool IsImplementing(Type type, Type type1)
        {
            return type.IsAssignableFrom(type1);
        }
        
        public static bool IsImplementingInterface(Type sourceType, Type interfaceType)
        {
            return sourceType.GetInterface(interfaceType.Name) != null;
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }


        public static Type GetGenericType(Type type)
        {
            const int genericIndex = 0;

            if (!type.IsGenericType)
            {
                return type;
            }

            Type[] genericArguments =type.GetGenericArguments();

            if (genericArguments.Length == 0)
            {
                throw new ArgumentException("type is not generic source");
            }

            return genericArguments[genericIndex];
        }

        public static bool IsConvertible(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof (Nullable<>)))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

		public static Func<object, object> MakeDelegateMethod(Type instanceType, MethodInfo method)
		{
			MethodInfo genericHelper = typeof(ReflectionHelper).GetMethod("DelegateMethodHelper", BindingFlags.Static | BindingFlags.NonPublic);
			
			// Now supply the type arguments
			MethodInfo constructedHelper = genericHelper.MakeGenericMethod(instanceType, method.ReturnType);

			// Now call it. The null argument is because it's a static method.
			object ret = constructedHelper.Invoke(null, new object[] { method });

			// Cast the result to the right kind of delegate and return it
			return (Func<object, object>)ret;
		}

		private static Func<object, object> DelegateMethodHelper<TTarget, TReturn>(MethodInfo method)
			where TTarget : class
		{
			// Convert the slow MethodInfo into a fast, strongly typed, open delegate
			Func<TTarget,TReturn> function = (Func<TTarget, TReturn>) Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

			// Now create a more weakly typed delegate which will call the strongly typed one
			Func<object, object> ret = (object target) => (object) function((TTarget)target);
			return ret;
		}
    }
}