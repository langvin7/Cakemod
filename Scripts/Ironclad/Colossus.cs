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

[HarmonyPatch(typeof(Colossus), MethodType.Constructor)]
public class ColossusConstructorPatch
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
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardRarity.Uncommon));
        
        return matcher.InstructionEnumeration();
    }
}


[HarmonyPatch(typeof(Colossus), "CanonicalVars", MethodType.Getter)]
public static class ColossusCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new BlockVar(4m, ValueProp.Move),
        new DynamicVar("Colossus", 1m)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class ColossusKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Colossus)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Colossus), "ExtraHoverTips", MethodType.Getter)]
public static class ColossusExtraHoverTipsPatch
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
[HarmonyPatch(typeof(Colossus), "OnPlay")]
public static class ColossusOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Colossus __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Colossus __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
 */

/* [HarmonyPatch(typeof(Colossus), "OnUpgrade")]
public static class ColossusOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Colossus __instance)
    {
        return false;
    }
} */