using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace cakemod.Scripts;

[HarmonyPatch(typeof(Thunderclap), "CanonicalVars", MethodType.Getter)]
public static class ThunderclapCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(4m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}

[HarmonyPatch(typeof(Thunderclap), "ExtraHoverTips", MethodType.Getter)]
public static class ThunderclapExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromCard<Thunder>()
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}

[HarmonyPatch(typeof(Thunderclap), "OnPlay")]
public static class ThunderclapOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Thunderclap __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Thunderclap __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).FromCard(__instance).TargetingAllOpponents(__instance.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        
        await PowerCmd.Apply<VulnerablePower>(__instance.CombatState.HittableEnemies, __instance.DynamicVars.Vulnerable.BaseValue, __instance.Owner.Creature, __instance);
        
        await PowerCmd.Apply<WeakPower>(__instance.CombatState.HittableEnemies, __instance.DynamicVars.Vulnerable.BaseValue, __instance.Owner.Creature, __instance);

        CardModel card = __instance.CombatState.CreateCard<Thunder>(__instance.Owner);
        CardCmd.Preview(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
    }
}

[HarmonyPatch(typeof(Thunderclap), "OnUpgrade")]
public static class ThunderclapOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Thunderclap __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        return false;
    }
}
