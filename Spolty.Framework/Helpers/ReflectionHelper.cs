using System;
using System.Reflection;

namespace Spolty.Framework.Helpers
{
    public static class ReflectionHelper
    {
        public const BindingFlags PublicMemberFlag = BindingFlags.GetField | BindingFlags.GetProperty |
                                                     BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        public const BindingFlags PrivateMemberFlag = BindingFlags.GetField | BindingFlags.GetProperty |
                                                      BindingFlags.DeclaredOnly | BindingFlags.NonPublic |
                                                      BindingFlags.Instance;

        public static MemberInfo GetMemberInfo(Type t, string name)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }
            MemberInfo[] res = t.GetMember(name, PublicMemberFlag | PrivateMemberFlag);
            if (res != null && res.Length > 0)
            {
                return res[0];
            }
            return null;
        }

        public static Type GetMemberType(MemberInfo mi)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }
            if (mi is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)mi;
                return fi.FieldType;
            }
            if (!(mi is PropertyInfo))
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", mi.GetType().Name));
            }
            PropertyInfo pi = (PropertyInfo) mi;
            return pi.PropertyType;
        }

        public static object GetValue(MemberInfo mi, object source)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mi is FieldInfo)
            {
                FieldInfo fi = (FieldInfo) mi;
                return fi.GetValue(source);
            }

            else if (mi is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo) mi;
                return pi.GetValue(source, null);
            }
            else
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", mi.GetType().Name));
            }
        }

        public static void SetValue(MemberInfo mi, object source, object value)
        {
            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (mi is FieldInfo)
            {
                FieldInfo fi = (FieldInfo)mi;
                fi.SetValue(source, value);
            }
            else if (mi is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)mi;
                pi.SetValue(source, value, null);
            }
            else
            {
                throw new NotSupportedException(String.Format("{0} is not supported.", mi.GetType().Name));
            }
        }

        public static bool IsImplementing(Type type, Type type1)
        {
            return type.IsAssignableFrom(type1);
        }
    }
}