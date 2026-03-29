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

/* [HarmonyPatch(typeof(DemonForm), MethodType.Constructor)]
public class DemonFormConstructorPatch
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

        // matcher.Advance(-3);
        // matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardRarity.Rare));
        
        return matcher.InstructionEnumeration();
    }
}
 */

[HarmonyPatch(typeof(DemonForm), "CanonicalVars", MethodType.Getter)]
public static class DemonFormCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new PowerVar<StrengthPower>(3m),
        new HpLossVar(2m)
     ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class DemonFormKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Ethereal ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is DemonForm)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(DemonForm), "ExtraHoverTips", MethodType.Getter)]
public static class DemonFormExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(DemonForm), "OnPlay")]
public static class DemonFormOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(DemonForm __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(DemonForm __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int missingEnergy = 3 - __instance.Owner.PlayerCombatState.Energy;
        if (missingEnergy > 0)
        {
            VfxCmd.PlayOnCreatureCenter(__instance.Owner.Creature, "vfx/vfx_bloody_impact");
            for (int i = 0; i < missingEnergy; i++)
            {
                await CreatureCmd.Damage(choiceContext, __instance.Owner.Creature, __instance.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, __instance);
            }
            await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
            SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_BloodWall");
            VfxCmd.PlayOnCreature(__instance.Owner.Creature, "vfx/vfx_blood_wall");
        }
        else
        {
            await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        }
        __instance.Owner.PlayerCombatState.LoseEnergy(__instance.Owner.PlayerCombatState.Energy>3?3:__instance.Owner.PlayerCombatState.Energy);
        await PowerCmd.Apply<DemonFormPower>(__instance.Owner.Creature, __instance.DynamicVars["StrengthPower"].BaseValue, __instance.Owner.Creature, __instance);
    }
}

[HarmonyPatch(typeof(AbstractModel), "AfterCardDrawn")]
public static class DemonFormAfterCardDrawnPatch
{
    [HarmonyPrefix]
    // 2. 将 __instance 的类型也改为 AbstractModel，并恢复你原本正确的参数列表
    public static void Prefix(AbstractModel __instance, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 3. 安全转换：判断当前触发事件的实例是不是 DemonForm，并且被抽到的卡恰好是它自己
        if (__instance is DemonForm demonForm && card == demonForm)
        {
            // 触发减费逻辑
            demonForm.EnergyCost.AddThisCombat(-demonForm.EnergyCost.GetWithModifiers(CostModifiers.All));
        }
    }
}

[HarmonyPatch(typeof(DemonForm), "OnUpgrade")]
public static class DemonFormOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(DemonForm __instance)
    {
        __instance.DynamicVars.HpLoss.UpgradeValueBy(-1m);
        return false;
    }
}
