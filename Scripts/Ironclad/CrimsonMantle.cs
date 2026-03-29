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
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(CrimsonMantle), MethodType.Constructor)]
public class CrimsonMantleConstructorPatch
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


[HarmonyPatch(typeof(CrimsonMantle), "CanonicalVars", MethodType.Getter)]
public static class CrimsonMantleCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new BlockVar(8m,ValueProp.Move)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class CrimsonMantleKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is CrimsonMantle)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(CrimsonMantle), "ExtraHoverTips", MethodType.Getter)]
public static class CrimsonMantleExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(CrimsonMantle), "OnPlay")]
public static class CrimsonMantleOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(CrimsonMantle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(CrimsonMantle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(__instance.Owner.Creature);
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CakeCrimsonMantlePower>(__instance.Owner.Creature, 1m, __instance.Owner.Creature, __instance);
        await CreatureCmd.GainBlock(__instance.Owner.Creature, __instance.DynamicVars.Block, cardPlay);
    }
}


[HarmonyPatch(typeof(CrimsonMantle), "OnUpgrade")]
public static class CrimsonMantleOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(CrimsonMantle __instance)
    {
        __instance.DynamicVars.Block.UpgradeValueBy(3m);
        return false;
    }
}