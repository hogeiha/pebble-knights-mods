using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace UniversalKingActions
{
    internal static class TypeFinder
    {
        public static Type Find(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static MethodInfo FindMethod(string typeName, string methodName)
        {
            var type = Find(typeName);
            if (type == null)
                return null;

            return AccessTools.Method(type, methodName);
        }

        public static MethodInfo FindAsyncStateMachineMoveNext(string typeName, string methodName)
        {
            var type = Find(typeName);
            if (type == null)
                return null;

            var method = AccessTools.Method(type, methodName);
            if (method != null)
            {
                var attribute = Attribute.GetCustomAttribute(
                    method,
                    typeof(AsyncStateMachineAttribute)) as AsyncStateMachineAttribute;
                if (attribute != null && attribute.StateMachineType != null)
                {
                    var moveNext = AccessTools.Method(attribute.StateMachineType, "MoveNext");
                    if (moveNext != null)
                        return moveNext;
                }
            }

            var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            var prefix = "<" + methodName + ">d__";
            for (var i = 0; i < nestedTypes.Length; i++)
            {
                if (!nestedTypes[i].Name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                var moveNext = AccessTools.Method(nestedTypes[i], "MoveNext");
                if (moveNext != null)
                    return moveNext;
            }

            return null;
        }
    }
}
