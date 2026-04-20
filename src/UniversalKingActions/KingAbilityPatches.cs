using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace UniversalKingActions
{
    internal static class KingAbilityPatches
    {
        private static MethodInfo IsKingGetter()
        {
            return TypeFinder.FindMethod("Player", "get_IsKing");
        }

        public static IEnumerable<CodeInstruction> TreatAsKing(IEnumerable<CodeInstruction> instructions)
        {
            var isKing = IsKingGetter();
            if (isKing == null)
                return instructions;

            var output = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(isKing))
                {
                    var pop = new CodeInstruction(OpCodes.Pop);
                    pop.labels.AddRange(instruction.labels);
                    output.Add(pop);
                    output.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    continue;
                }

                output.Add(instruction);
            }

            return output;
        }
    }

    [HarmonyPatch]
    internal static class ItemAuraCheckPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("Item", "AuraCheck");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }

    [HarmonyPatch]
    internal static class ItemHolderCheckTraitConditionsPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("ItemHolder", "CheckTraitConditions");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }

    [HarmonyPatch]
    internal static class TraitHolderTryUpgradePatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("Traits.TraitHolder", "TryUpgrade");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }

    [HarmonyPatch]
    internal static class TraitHolderUpgradeTraitPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("Traits.TraitHolder", "UpgradeTrait");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }

    [HarmonyPatch]
    internal static class TraitHolderGetTraitsForEvolutionPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("Traits.TraitHolder", "GetTraitsForEvolution");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }

    [HarmonyPatch]
    internal static class EvolutionHolderGetRandomTraitsPatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("EvolutionHolder", "GetRandomTraits");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return KingAbilityPatches.TreatAsKing(instructions);
        }
    }
}
