using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace UniversalKingActions
{
    [HarmonyPatch]
    internal static class EvolutionUiShowUiAsyncPatch
    {
        private static MethodBase TargetMethod()
        {
            var stateMachineType = TypeFinder.Find("EvolutionUI+<ShowUIAsync>d__23");
            return stateMachineType == null ? null : AccessTools.Method(stateMachineType, "MoveNext");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);
            var localPlayerGetter = TypeFinder.FindMethod("LocalPlayer", "get_Player");
            var playerIdGetter = TypeFinder.FindMethod("Player", "get_Id");
            var canLocalSelect = AccessTools.Method(
                typeof(EvolutionSelectionHelper),
                "CanLocalSelectEvolution");

            if (localPlayerGetter == null || playerIdGetter == null || canLocalSelect == null)
                return list;

            for (var i = 0; i < list.Count - 2; i++)
            {
                if (!list[i].Calls(localPlayerGetter) ||
                    !list[i + 1].Calls(playerIdGetter) ||
                    list[i + 2].opcode != OpCodes.Ceq)
                    continue;

                var labels = list[i].labels;
                list[i] = new CodeInstruction(OpCodes.Call, canLocalSelect);
                list[i].labels.AddRange(labels);

                list[i + 1].opcode = OpCodes.Nop;
                list[i + 1].operand = null;
                list[i + 2].opcode = OpCodes.Nop;
                list[i + 2].operand = null;
            }

            return list;
        }
    }
}
