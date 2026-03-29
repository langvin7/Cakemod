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
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Events;

namespace cakemod.Scripts;

[HarmonyPatch(typeof(HowlFromBeyond), "CanonicalVars", MethodType.Getter)]
public static class HowlFromBeyondCanonicalVarsPatch
{
	private static readonly DynamicVar[] ModifiedVars =
    [
		new HpLossVar(3m),
		new DamageVar(20m, ValueProp.Move)
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<DynamicVar> __result)
	{
		__result = ModifiedVars;
	}
}


// 将目标类改为 AbstractModel
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterShuffle))]
public static class HowlFromBeyond_AfterShuffle_Patch
{
    // __instance 的类型需要改为 AbstractModel，与被 Patch 的类保持一致
    public static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player shuffler, ref Task __result)
    {
        // 游戏里所有的 Model 洗牌时都会走这里，但我们只对 HowlFromBeyond 生效
        if (__instance is HowlFromBeyond howlCard)
        {
            // 将原方法返回的 Task 替换为我们的自定义异步 Task
            __result = CustomAfterShuffleLogic(__result, howlCard, choiceContext, shuffler);
        }
    }

    // 在这里编写你想要为 HowlFromBeyond 添加的 AfterShuffle 专属逻辑
    private static async Task CustomAfterShuffleLogic(Task baseTask, HowlFromBeyond card, PlayerChoiceContext choiceContext, Player shuffler)
    {
        // 1. 等待原本的 Task 执行完毕（虽然 AbstractModel 里是 CompletedTask，但兼容其他可能存在的 Mod）
        if (baseTask != null)
        {
            await baseTask;
        }

        if (shuffler == card.Owner && card.Pile?.Type != PileType.Exhaust)
        {
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(card.Owner.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(choiceContext, card.Owner.Creature, card.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
        await CardCmd.Exhaust(choiceContext, card);
        }
    }
}

[HarmonyPatch(typeof(HowlFromBeyond), "OnUpgrade")]
public static class HowlFromBeyondOnUpgradePatch
{
	[HarmonyPostfix]
	public static void Postfix(HowlFromBeyond __instance)
	{
		__instance.DynamicVars.HpLoss.UpgradeValueBy(-1m);
	}
}