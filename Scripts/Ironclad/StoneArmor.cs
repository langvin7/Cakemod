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

/* [HarmonyPatch(typeof(StoneArmor), MethodType.Constructor)]
public class StoneArmorConstructorPatch
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
}
 */

[HarmonyPatch(typeof(StoneArmor), "CanonicalVars", MethodType.Getter)]
public static class StoneArmorCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new PowerVar<PlatingPower>(3m)
     ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class StoneArmorKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is StoneArmor)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(StoneArmor), "ExtraHoverTips", MethodType.Getter)]
public static class StoneArmorExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(StoneArmor), "OnPlay")]
public static class StoneArmorOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(StoneArmor __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(StoneArmor __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PlatingPower>(__instance.Owner.Creature, __instance.DynamicVars["PlatingPower"].BaseValue, __instance.Owner.Creature, __instance);
        await PowerCmd.Apply<ArtifactPower>(__instance.Owner.Creature,1m, __instance.Owner.Creature, __instance);
    }
}



[HarmonyPatch(typeof(StoneArmor), "OnUpgrade")]
public static class StoneArmorOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(StoneArmor __instance)
    {
        __instance.DynamicVars["PlatingPower"].UpgradeValueBy(2m);
        __instance.AddKeyword(CardKeyword.Innate);
        return false;
    }
}