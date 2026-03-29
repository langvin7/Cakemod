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

[HarmonyPatch(typeof(Juggernaut), MethodType.Constructor)]
public class JuggernautConstructorPatch
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
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_1));
        
        return matcher.InstructionEnumeration();
    }
}


[HarmonyPatch(typeof(Juggernaut), "CanonicalVars", MethodType.Getter)]
public static class JuggernautCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new PowerVar<IronWavePower>(3m),
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
public static class JuggernautKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Juggernaut)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

[HarmonyPatch(typeof(Juggernaut), "ExtraHoverTips", MethodType.Getter)]
public static class JuggernautExtraHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = __result.Append(HoverTipFactory.FromCard<IronWave>());
    }
}


[HarmonyPatch(typeof(Juggernaut), "OnPlay")]
public static class JuggernautOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Juggernaut __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Juggernaut __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CakeJuggernautPower>(__instance.Owner.Creature, 1, __instance.Owner.Creature, __instance);
        await PowerCmd.Apply<PlatingPower>(__instance.Owner.Creature, __instance.DynamicVars["PlatingPower"].BaseValue, __instance.Owner.Creature, __instance);
        await PowerCmd.Apply<IronWavePower>(__instance.Owner.Creature, __instance.DynamicVars["IronWavePower"].BaseValue, __instance.Owner.Creature, __instance);
    }
}


[HarmonyPatch(typeof(Juggernaut), "OnUpgrade")]
public static class JuggernautOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Juggernaut __instance)
    {
        __instance.DynamicVars["PlatingPower"].UpgradeValueBy(1m);
        __instance.DynamicVars["IronWavePower"].UpgradeValueBy(1m);
        return false;
    }
}
