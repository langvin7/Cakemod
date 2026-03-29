using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.GameActions; 
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection; 
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Multiplayer;



namespace cakemod.Scripts;
public static class CustomCardSelectCmd
{
    // 缓存反射获取的私有方法，提高性能
    private static readonly MethodInfo ShouldSelectLocalCardMethod = typeof(CardSelectCmd).GetMethod("ShouldSelectLocalCard", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo LogChoiceMethod = typeof(CardSelectCmd).GetMethod("LogChoice", BindingFlags.NonPublic | BindingFlags.Static);

    public static async Task<IEnumerable<CardModel>> FromDiscard(PlayerChoiceContext context, Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel source)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }

        bool shouldSelectLocalCard = (bool)ShouldSelectLocalCardMethod.Invoke(null, new object[] { player });

        if (shouldSelectLocalCard)
        {
            NPlayerHand.Instance?.CancelAllCardPlay();
        }

        // 修改点：从 PileType.Discard 获取卡牌
        List<CardModel> list = PileType.Discard.GetPile(player).Cards.Where(filter ?? ((CardModel _) => true)).ToList();
        
        IEnumerable<CardModel> result;
        if (list.Count == 0)
        {
            result = list;
        }
        else if (!prefs.RequireManualConfirmation && list.Count <= prefs.MinSelect)
        {
            result = list;
        }
        else if (CardSelectCmd.Selector != null)
        {
            result = await CardSelectCmd.Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
        }
        else
        {
            uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
            await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.CancelPlayCardActions );
            if (shouldSelectLocalCard)
            {
                // ⚠️ 注意：原版 FromHand 使用的是 NCombatRoom.Instance.Ui.Hand.SelectCards
                // 但那是专门针对“手牌区”的 UI。如果要从弃牌堆选牌，使用 Hand UI 会导致玩家无法看到弃牌堆的牌。
                // 建议替换为网格选择界面（参考 FromSimpleGrid 或 FromDeckGeneric），如下所示：
                
                NSimpleCardSelectScreen nSimpleCardSelectScreen = NSimpleCardSelectScreen.Create(list, prefs);
                NOverlayStack.Instance.Push(nSimpleCardSelectScreen);
                result = (await nSimpleCardSelectScreen.CardsSelected()).ToList();
                
                // 如果你坚持要用原版的 Hand UI（可能会有显示Bug），可以取消下面两行的注释并删掉上面的网格代码：
                // result = await NCombatRoom.Instance.Ui.Hand.SelectCards(prefs, filter, source);
                
                RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableCombatCards(result));
            }
            else
            {
                result = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsCombatCards();
            }

            await context.SignalPlayerChoiceEnded();
        }

        // 调用私有的 LogChoice
        LogChoiceMethod.Invoke(null, new object[] { player, result });

        return result;
    }


public static async Task<IEnumerable<CardModel>> FromDraw(
    PlayerChoiceContext context, 
    Player player, 
    CardSelectorPrefs prefs, 
    Func<CardModel, bool>? filter, 
    AbstractModel source, 
    int? topCount = null) // 修改点1：增加 topCount 参数，表示要看牌堆顶几张牌
{
    if (CombatManager.Instance.IsOverOrEnding)
    {
        return Array.Empty<CardModel>();
    }

    bool shouldSelectLocalCard = (bool)ShouldSelectLocalCardMethod.Invoke(null, new object[] { player });

    if (shouldSelectLocalCard)
    {
        NPlayerHand.Instance?.CancelAllCardPlay();
    }

    // 获取玩家的抽牌堆
    IEnumerable<CardModel> drawPile = PileType.Draw.GetPile(player).Cards;

    // 修改点2：如果传入了 topCount，则只截取牌堆顶的 X 张牌
    if (topCount.HasValue && topCount.Value > 0)
    {
        // 注意：这里假设列表的末尾（最后面的元素）是牌堆顶。
        // 如果你的游戏底层逻辑将索引 0 作为牌堆顶，请将其改为： drawPile = drawPile.Take(topCount.Value);
        drawPile = drawPile.Take(topCount.Value);
        
        // 如果你需要让牌堆顶的牌在UI界面上排在最前面，可能还需要加上 .Reverse()
        // drawPile = drawPile.Reverse(); 
    }

    // 修改点3：应用过滤器并转换为列表
    List<CardModel> list = drawPile.Where(filter ?? ((CardModel _) => true)).ToList();
    
    IEnumerable<CardModel> result;
    if (list.Count == 0)
    {
        result = list;
    }
    else if (!prefs.RequireManualConfirmation && list.Count <= prefs.MinSelect)
    {
        result = list;
    }
    else if (CardSelectCmd.Selector != null)
    {
        result = await CardSelectCmd.Selector.GetSelectedCards(list, prefs.MinSelect, prefs.MaxSelect);
    }
    else
    {
        uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
        await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.CancelPlayCardActions);
        if (shouldSelectLocalCard)
        {
            // 使用网格选择界面展示这 X 张牌
            NSimpleCardSelectScreen nSimpleCardSelectScreen = NSimpleCardSelectScreen.Create(list, prefs);
            NOverlayStack.Instance.Push(nSimpleCardSelectScreen);
            result = (await nSimpleCardSelectScreen.CardsSelected()).ToList();
            
            RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableCombatCards(result));
        }
        else
        {
            result = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsCombatCards();
        }

        await context.SignalPlayerChoiceEnded();
    }

    // 调用私有的 LogChoice
    LogChoiceMethod.Invoke(null, new object[] { player, result });

    return result;
}

}
