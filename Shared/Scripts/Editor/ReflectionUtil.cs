using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nexcide {

    public static class ReflectionUtil {

        public static Type GetTypeFromAssembly(string typeName, string assemblyName = null) {
            Type type = Type.GetType(typeName);

            if (type == null) {
                IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

                if (assemblyName != null) {
                    assemblies = assemblies.Where(x => x.FullName.Contains(assemblyName));
                }

                foreach (Assembly assembly in assemblies) {
                    type = assembly.GetType(typeName);

                    if (type != null) {
                        return type;
                    }
                }
            }

            return type;
        }

        public static T CreateDelegate<T>(this Type type, string methodName) where T : Delegate {
            MethodInfo methodInfo = type.GetMethod(methodName);

            if (methodInfo != null) {
                return (T)Delegate.CreateDelegate(typeof(T), methodInfo);
            } else {
                Log.w($"Couldn't get method '{methodName}' from type: {type}");
            }

            return null;
        }

        public static T CreateDelegate<T>(this MethodInfo methodInfo) where T : Delegate {
            return (T)Delegate.CreateDelegate(typeof(T), methodInfo);
        }

        public static Func<S, T> CreateDelegate<S, T>(this FieldInfo fieldInfo) {
            ParameterExpression instExp = Expression.Parameter(typeof(S));
            MemberExpression fieldExp = Expression.Field(instExp, fieldInfo);
            return Expression.Lambda<Func<S, T>>(fieldExp, instExp).Compile();
        }
    }
}
