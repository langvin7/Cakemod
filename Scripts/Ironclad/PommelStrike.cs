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
using System.Reflection.Metadata.Ecma335;

namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(PommelStrike), MethodType.Constructor)]
public class PommelStrikeConstructorPatch
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
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0));
        
        return matcher.InstructionEnumeration();
    }
} */

/* 
[HarmonyPatch(typeof(PommelStrike), "CanonicalVars", MethodType.Getter)]
public static class PommelStrikeCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}
 */

/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class PommelStrikeKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is PommelStrike)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(PommelStrike), "ExtraHoverTips", MethodType.Getter)]
public static class PommelStrikeExtraHoverTipsPatch
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
[HarmonyPatch(typeof(PommelStrike), "OnPlay")]
public static class PommelStrikeOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PommelStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(PommelStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
 */

[HarmonyPatch(typeof(PommelStrike), "OnUpgrade")]
public static class PommelStrikeOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(PommelStrike __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(6m);
        return false;
    }
}