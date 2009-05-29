using System;
using System.Collections.Generic;
using System.Reflection;

namespace Spolty.Framework.Activators
{
    public static class SpoltyActivator
    {
        private static readonly Dictionary<string, ConstructorInfo> _dictionary =
            new Dictionary<string, ConstructorInfo>();

        public static T CreateInstance<T>(string typeName) where T : class
        {
            return CreateInstance<T>(typeName, null);
        }

        public static T CreateInstance<T>(string typeName, object[] parameters) where T : class
        {
            ConstructorInfo ctor;
            if (!_dictionary.TryGetValue(typeName, out ctor))
            {
                Type type = Type.GetType(typeName);

                if (type == null)
                {
                    throw new TypeLoadException(String.Format("Provider name: {0} not found", typeName));
                }

                Type[] types;
                if (parameters == null)
                    types = new Type[] {};
                else
                {
                    types = new Type[parameters.Length];
                    for (int i = 0; i < types.Length; i++)
                    {
                        types[i] = parameters[i].GetType();
                    }
                }
                ctor = type.GetConstructor(types);

                if (ctor == null)
                {
                    throw new NotSupportedException(String.Format("Default constructor for provider: {0} not found",
                                                                  typeName));
                }

                _dictionary.Add(typeName, ctor);
            }

            object instance = ctor.Invoke(parameters);
            var provider = instance as T;

            if (provider == null)
            {
                throw new InvalidCastException(String.Format("Type: {0} cannot be casted to type: {1}",
                                                             instance.GetType().FullName,
                                                             typeof (T).FullName));
            }

            return provider;
        }
    }
}