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

[HarmonyPatch(typeof(BurningPact), MethodType.Constructor)]
public class BurningPactConstructorPatch
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

        matcher.Advance(-3);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardRarity.Rare));
        
        return matcher.InstructionEnumeration();
    }
}



[HarmonyPatch(typeof(BurningPact), "CanonicalVars", MethodType.Getter)]
public static class BurningPactCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new CardsVar(3)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class BurningPactKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is BurningPact)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(BurningPact), "ExtraHoverTips", MethodType.Getter)]
public static class BurningPactExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/

/* 
[HarmonyPatch(typeof(BurningPact), "OnPlay")]
public static class BurningPactOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(BurningPact __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(BurningPact __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
 */

/* [HarmonyPatch(typeof(BurningPact), "OnUpgrade")]
public static class BurningPactOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(BurningPact __instance)
    {
        return false;
    }
} */