using System;
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
    }
}