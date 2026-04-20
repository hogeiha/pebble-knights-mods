using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace UniversalKingActions
{
    [HarmonyPatch]
    internal static class PlayerItemCheckConditionPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("PlayerItemHolderController", "CheckCondition");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var networkServerType = TypeFinder.Find("Mirror.NetworkServer");
            var activeGetter = networkServerType == null ? null : AccessTools.PropertyGetter(networkServerType, "active");
            var output = new List<CodeInstruction>();

            foreach (var instruction in instructions)
            {
                if (activeGetter != null && instruction.Calls(activeGetter))
                {
                    var replacement = new CodeInstruction(OpCodes.Ldc_I4_1);
                    replacement.labels.AddRange(instruction.labels);
                    output.Add(replacement);
                    continue;
                }

                output.Add(instruction);
            }

            return output;
        }
    }

    [HarmonyPatch]
    internal static class PlayerItemPickupItemPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("PlayerItemHolderController", "PickupItem");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);
            var itemTypeGetter = TypeFinder.FindMethod("Item", "get_Type");

            if (itemTypeGetter == null)
                return list;

            for (var i = 3; i < list.Count; i++)
            {
                if (!CallsFlagExtensions(list[i], "HasFlagNonAlloc"))
                    continue;

                if (!list[i - 2].Calls(itemTypeGetter))
                    continue;

                if (list[i - 1].opcode != OpCodes.Ldc_I4_1)
                    continue;

                if (!IsLdarg1(list[i - 3]))
                    continue;

                list[i - 3].opcode = OpCodes.Nop;
                list[i - 3].operand = null;
                list[i - 2].opcode = OpCodes.Nop;
                list[i - 2].operand = null;
                list[i - 1].opcode = OpCodes.Nop;
                list[i - 1].operand = null;
                list[i].opcode = OpCodes.Ldc_I4_0;
                list[i].operand = null;
                break;
            }

            return list;
        }

        private static bool IsLdarg1(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Ldarg_1 ||
                   (instruction.opcode == OpCodes.Ldarg_S && instruction.operand != null && instruction.operand.ToString() == "item");
        }

        private static bool CallsFlagExtensions(CodeInstruction instruction, string methodName)
        {
            var method = instruction.operand as MethodInfo;
            return method != null &&
                   method.Name == methodName &&
                   method.DeclaringType != null &&
                   method.DeclaringType.FullName == "FlagExtensionsGenerated";
        }
    }
}
