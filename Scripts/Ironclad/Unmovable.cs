using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Reflection.Emit;

namespace cakemod.Scripts;

[HarmonyPatch(typeof(Unmovable), MethodType.Constructor)]
public class UnmovableConstructorPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.DeclaredConstructor(
                typeof(CardModel), 
                new[] { typeof(int), typeof(CardType), typeof(CardRarity), typeof(TargetType), typeof(bool) }
            ))
        );

        if (matcher.IsInvalid) return instructions;

        matcher.Advance(-5);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_2));
        
        return matcher.InstructionEnumeration();
    }
}

/* 
[HarmonyPatch(typeof(Unmovable), "CanonicalVars", MethodType.Getter)]
public static class UnmovableCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new PowerVar<CakeUnmovablePower>(2m)
     ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}
 */


[HarmonyPatch(typeof(CardModel), "CanonicalVars", MethodType.Getter)]
public static class UnmovableCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedKeywords = [ 
        new PowerVar<CakeUnmovablePower>(2m) 
        ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<DynamicVar> __result)
    {
        if (__instance is Unmovable)
        {
            __result = ModifiedKeywords;
        }
    }
}




/*
[HarmonyPatch(typeof(Unmovable), "ExtraHoverTips", MethodType.Getter)]
public static class UnmovableExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Unmovable), "OnPlay")]
public static class UnmovableOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Unmovable __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Unmovable __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CakeUnmovablePower>(__instance.Owner.Creature, __instance.DynamicVars["CakeUnmovablePower"].BaseValue, __instance.Owner.Creature, __instance);
    }
}


[HarmonyPatch(typeof(Unmovable), "OnUpgrade")]
public static class UnmovableOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Unmovable __instance)
    {
        __instance.AddKeyword(CardKeyword.Innate);
        return false;
    }
}

