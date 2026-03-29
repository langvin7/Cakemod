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

/* [HarmonyPatch(typeof(MoltenFist), MethodType.Constructor)]
public class MoltenFistConstructorPatch
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


[HarmonyPatch(typeof(MoltenFist), "CanonicalVars", MethodType.Getter)]
public static class MoltenFistCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(10m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class MoltenFistKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is MoltenFist)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(MoltenFist), "ExtraHoverTips", MethodType.Getter)]
public static class MoltenFistExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(MoltenFist), "OnPlay")]
public static class MoltenFistOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MoltenFist __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(MoltenFist __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).FromCard(__instance).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_molten_fist", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        int num = (cardPlay.Target.IsAlive ? cardPlay.Target.GetPowerAmount<VulnerablePower>() : 0);
        if (num > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, __instance.IsUpgraded?2*num:num, __instance.Owner.Creature, __instance);
        }
        else
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target,__instance.DynamicVars.Vulnerable.BaseValue, __instance.Owner.Creature, __instance);
        }
    }
}


[HarmonyPatch(typeof(MoltenFist), "OnUpgrade")]
public static class MoltenFistOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(MoltenFist __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(4m);
        __instance.DynamicVars.Vulnerable.UpgradeValueBy(1m);
        return false;
    }
}