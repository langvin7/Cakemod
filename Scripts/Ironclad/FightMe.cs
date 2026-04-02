using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Reflection.Emit;

namespace cakemod.Scripts;

 [HarmonyPatch(typeof(FightMe), MethodType.Constructor)]
    public class FightMeConstructorPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 使用 CodeMatcher 可以非常安全且精准地定位和修改 IL
            var matcher = new CodeMatcher(instructions);

            // 寻找调用基类 CardModel 构造函数的指令
            // CardModel 的构造函数签名为: (int, CardType, CardRarity, TargetType, bool)
            // 注意: bool shouldShowInCardLibrary = true 是默认参数，编译器会自动在调用方补全这个 true
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, AccessTools.DeclaredConstructor(
                    typeof(CardModel), 
                    new[] { typeof(int), typeof(CardType), typeof(CardRarity), typeof(TargetType), typeof(bool) }
                ))
            );

            if (matcher.IsInvalid)
            {
                // 如果因为游戏更新导致没找到，直接返回原指令防止崩溃
                return instructions;
            }

            // 此时 matcher 停在 Call 指令上。
            // 往回倒退 5 步，正好是第一个参数 canonicalEnergyCost 的压栈指令 (ldc.i4.1)
            // 栈的推入顺序是: this -> cost -> type -> rarity -> targetType -> shouldShow
            matcher.Advance(-5);

            // 将费用修改为你想要的值，例如修改为 0 费
            matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_2));

                    // 2. 前进 1 步 (相对 Call 是 -4)，到达第 2 个参数 (CardType)
            matcher.Advance(1);
            matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardType.Skill)); // 修改为 Power
            
            // 如果你想改成 2 费，使用: new CodeInstruction(OpCodes.Ldc_I4_2)
            // 如果你想改成 3 费，使用: new CodeInstruction(OpCodes.Ldc_I4_3)
            matcher.Advance(2);
            matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)TargetType.Self)); // 修改为 Self

            return matcher.InstructionEnumeration();
        }
    }

[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("CanonicalKeywords", MethodType.Getter)]
public static class FightMeKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords =
    [
        CardKeyword.Exhaust
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is FightMe)
        {
            __result = ModifiedKeywords;
        }
    }
}


[HarmonyPatch(typeof(FightMe), "OnPlay")]
public static class FightMeOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(FightMe __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(FightMe __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        foreach (CardModel item in PileType.Hand.GetPile(__instance.Owner).Cards.ToList())
        {
            await CardPileCmd.Add(item, PileType.Draw);
        }
        await CardPileCmd.Shuffle(choiceContext, __instance.Owner);

        CardPile drawPile = PileType.Draw.GetPile(__instance.Owner);
        IEnumerable<CardModel> attackCards = drawPile.Cards.Where(c => c.Type == CardType.Attack && !c.EnergyCost.CostsX && c.EnergyCost.GetWithModifiers(CostModifiers.All) >= 2);
        CardModel highestCostAttack = attackCards.ToList()
            .UnstableShuffle(__instance.Owner.RunState.Rng.CombatCardSelection)
            .OrderByDescending(c => c.EnergyCost.GetWithModifiers(CostModifiers.All))
            .FirstOrDefault();
        
        if (highestCostAttack != null)
        {
            await CardPileCmd.Add(highestCostAttack, PileType.Hand);
            highestCostAttack.EnergyCost.SetThisTurnOrUntilPlayed(0);
        }
    }
}

[HarmonyPatch(typeof(FightMe), "OnUpgrade")]
public static class FightMeOnUpgradePatch
{
	[HarmonyPrefix]
	public static bool Prefix(FightMe __instance)
	{
		__instance.RemoveKeyword(CardKeyword.Exhaust);
		return false;
	}
}
