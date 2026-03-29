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

/* [HarmonyPatch(typeof(Dominate), MethodType.Constructor)]
public class DominateConstructorPatch
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
[HarmonyPatch(typeof(Dominate), "CanonicalVars", MethodType.Getter)]
public static class DominateCanonicalVarsPatch
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
public static class DominateKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Dominate)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Dominate), "ExtraHoverTips", MethodType.Getter)]
public static class DominateExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Dominate), "OnPlay")]
public static class DominateOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Dominate __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Dominate __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int strengthToApply = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<StrengthPower>(__instance.Owner.Creature, strengthToApply, __instance.Owner.Creature, __instance);
        await CreatureCmd.Heal(__instance.Owner.Creature, strengthToApply);
    }
}


[HarmonyPatch(typeof(Dominate), "OnUpgrade")]
public static class DominateOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Dominate __instance)
    {
        __instance.EnergyCost.UpgradeBy(-1);
        return false;
    }
}