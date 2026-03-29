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

[HarmonyPatch(typeof(BattleTrance), MethodType.Constructor)]
public class BattleTranceConstructorPatch
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
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardRarity.Common));
        
        return matcher.InstructionEnumeration();
    }
}


[HarmonyPatch(typeof(BattleTrance), "CanonicalVars", MethodType.Getter)]
public static class BattleTranceCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new HpLossVar(2m),
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
public static class BattleTranceKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is BattleTrance)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(BattleTrance), "ExtraHoverTips", MethodType.Getter)]
public static class BattleTranceExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(BattleTrance), "OnPlay")]
public static class BattleTranceOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(BattleTrance __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(BattleTrance __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(__instance.Owner.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(choiceContext, __instance.Owner.Creature, __instance.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, __instance);
        await CardPileCmd.Draw(choiceContext, __instance.DynamicVars.Cards.BaseValue, __instance.Owner);
        await PowerCmd.Apply<NoDrawPower>(__instance.Owner.Creature, 1m, __instance.Owner.Creature, __instance);
    }
}


[HarmonyPatch(typeof(BattleTrance), "OnUpgrade")]
public static class BattleTranceOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(BattleTrance __instance)
    {
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        __instance.DynamicVars.HpLoss.UpgradeValueBy(-1m);
        return false;
    }
}