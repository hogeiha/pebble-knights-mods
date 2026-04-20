using System;
using System.Reflection;
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
    }
}
