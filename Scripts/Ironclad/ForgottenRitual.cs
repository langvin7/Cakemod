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
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.ValueProps;
using System.Reflection.Emit;
using MegaCrit.Sts2.Core.Localization;



using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(ForgottenRitual), MethodType.Constructor)]
public class ForgottenRitualConstructorPatch
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
[HarmonyPatch(typeof(ForgottenRitual), "CanonicalVars", MethodType.Getter)]
public static class ForgottenRitualCanonicalVarsPatch
{
	private static readonly DynamicVar[] ModifiedVars =
    [
		new BlockVar(6m, ValueProp.Move),
		new CardsVar(2)
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<DynamicVar> __result)
	{
		__result = ModifiedVars;
	}
}

/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class ForgottenRitualKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is ForgottenRitual)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("CanonicalKeywords", MethodType.Getter)]
public static class ForgottenRitualKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords =
    [
        CardKeyword.Exhaust
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is ForgottenRitual)
        {
            __result = ModifiedKeywords;
        }
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "OnPlay")]
public static class ForgottenRitualOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ForgottenRitual __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }


    private static async Task PatchedOnPlay(ForgottenRitual __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LocString prompt = Traverse.Create(__instance).Property("SelectionScreenPrompt").GetValue<LocString>();
        foreach (CardModel item in await CustomCardSelectCmd.FromDiscard(prefs: new CardSelectorPrefs(prompt, 0, __instance.DynamicVars.Cards.IntValue), context: choiceContext, player: __instance.Owner, filter: null, source: __instance))
		{
            if (item.EnergyCost.GetWithModifiers(CostModifiers.All) >= 1 && !item.EnergyCost.CostsX)
            {
                int newBaseCost = item.EnergyCost.GetWithModifiers(CostModifiers.All) - 1;
                item.EnergyCost.SetThisTurnOrUntilPlayed(newBaseCost);
            }
			await CardPileCmd.Add(item, PileType.Hand);
		}
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterCardChangedPilesLate))]
public static class ForgottenRitual_PileChange_Patch
{
    // 使用 Postfix 修改返回的 Task
    // 注意：参数名必须与原方法一致（card, oldPileType, source）

    private static bool _isTriggering = false;

    public static void Postfix(AbstractModel __instance, ref Task __result, CardModel card, PileType oldPileType, AbstractModel source)
    {
        // 【核心修复1】：必须确保当前收到通知的实体（__instance）就是这张回旋镖卡牌（card）本身！
        if (__instance == card && card is ForgottenRitual forgottenRitual)
        {
            __result = ExecuteforgottenRitualLogicAsync(__result, forgottenRitual, oldPileType);
        }
    }

    // 自定义的异步追加逻辑
    private static async Task ExecuteforgottenRitualLogicAsync(Task originalTask, ForgottenRitual forgottenRitual, PileType oldPileType)
    {
        // 确保游戏原有的逻辑先执行完毕
        if (originalTask != null)
        {
            await originalTask;
        }

        if (_isTriggering) return;
        // 3. 根据你提供的信息，通过 pile 成员获取当前所在的牌堆类型
        // 加上 ?. 防止卡牌被销毁或移除时 pile 为 null 导致报错
        // 注意：C# 严格区分大小写，如果源码是大写 P，请改为 forgottenRitual.Pile.Type
        PileType currentPile = forgottenRitual.Pile?.Type ?? PileType.None;

        // 4. 判断是否满足条件：弃牌堆 -> 抽牌堆，或 抽牌堆 -> 弃牌堆
        bool isDiscard = (currentPile == PileType.Discard);

        if (isDiscard)
        {
            try
            {
                // 上锁
                _isTriggering = true;

                await CreatureCmd.GainBlock(forgottenRitual.Owner.Creature, forgottenRitual.DynamicVars.Block,null);

            }
            finally
            {
                // 无论是否发生异常，最后必须解锁
                _isTriggering = false;
            }
        }
    }
}



[HarmonyPatch(typeof(ForgottenRitual), "OnUpgrade")]
public static class ForgottenRitualOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(ForgottenRitual __instance)
    {
        __instance.DynamicVars.Block.UpgradeValueBy(3m);
        return false;
    }
}

[HarmonyPatch(typeof(ForgottenRitual), "ShouldGlowGoldInternal", MethodType.Getter)]
public static class ForgottenRitualShouldGlowGoldPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
