using HarmonyLib;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;

namespace cakemod.Scripts.function;

/// <summary>
/// 为所有卡牌描述注入 {DeckStrikeDiv3} 变量，值为玩家牌组内打击牌数量 / 3。
/// 战斗中读取所有牌堆，战斗外读取牌组。
/// </summary>
[HarmonyPatch(typeof(CardModel), "AddExtraArgsToDescription")]
public static class DeckStrikeCountPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, LocString description)
    {
        IRunState? runState = __instance.RunState;
        var owner = LocalContext.GetMe(runState);
        if (owner == null)
        {
            description.Add("DeckStrikeDiv3", 0m);
            return;
        }

        int strikeCount = owner.Deck.Cards.Count(c => c.Tags.Contains(CardTag.Strike)) + 1;
        description.Add("DeckStrikeDiv3", (decimal)(strikeCount / 3));
    }
}
