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

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

[HarmonyPatch(typeof(IronWave), MethodType.Constructor)]
public class IronWaveConstructorPatch
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

        // 1. 退回 5 步，到达第 1 个参数 (int cost)
        matcher.Advance(-5);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0)); // 保持你原来的修改，将其设为 0
        
        // 2. 前进 1 步 (相对 Call 是 -4)，到达第 2 个参数 (CardType)
        matcher.Advance(1);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardType.Power)); // 修改为 Power

        matcher.Advance(1);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardRarity.Uncommon)); // 修改为Uncommon

        
        matcher.Advance(1);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)TargetType.Self)); // 修改为 Self
        
        return matcher.InstructionEnumeration();
    }
}



[HarmonyPatch(typeof(IronWave), "CanonicalVars", MethodType.Getter)]
public static class IronWaveCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(6m, ValueProp.Move)
     ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class IronWaveKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is IronWave)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(IronWave), "ExtraHoverTips", MethodType.Getter)]
public static class IronWaveExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(IronWave), "OnPlay")]
public static class IronWaveOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(IronWave __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(IronWave __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<IronWavePower>(__instance.Owner.Creature, __instance.DynamicVars.Damage.BaseValue, __instance.Owner.Creature, __instance);
    }
}


[HarmonyPatch(typeof(IronWave), "OnUpgrade")]
public static class IronWaveOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(IronWave __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        return false;
    }
}