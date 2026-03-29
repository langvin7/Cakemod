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
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Reflection.Emit;

namespace cakemod.Scripts;

//[HarmonyPatch(typeof(Dismantle), MethodType.Constructor)]
/* public class DismantleConstructorPatch
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


[HarmonyPatch(typeof(Dismantle), "CanonicalVars", MethodType.Getter)]
public static class DismantleCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(7m, ValueProp.Move)
     ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class DismantleKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Dismantle)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Dismantle), "ExtraHoverTips", MethodType.Getter)]
public static class DismantleExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Dismantle), "OnPlay")]
public static class DismantleOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Dismantle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Dismantle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        List<CardModel> attackCards = PileType.Draw.GetPile(__instance.Owner).Cards
        .Where(c => c.Type == CardType.Attack)
        .ToList();
		CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, attackCards, __instance.Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1))).FirstOrDefault();
        int hitCount = 1;
        if(cardModel != null)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
            hitCount = 2;
        }
        await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).WithHitCount(hitCount).FromCard(__instance)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }
}


/* [HarmonyPatch(typeof(Dismantle), "OnUpgrade")]
public static class DismantleOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Dismantle __instance)
    {
        return false;
    }
} */

[HarmonyPatch(typeof(Dismantle), "ShouldGlowGoldInternal", MethodType.Getter)]
public static class DismantleShouldGlowGoldPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false; // 返回 false 拦截原方法的执行
    }
}
