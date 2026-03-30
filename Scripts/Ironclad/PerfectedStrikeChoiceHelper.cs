using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using cakemod.Scripts;
using cakemod.Scripts.function;
using MegaCrit.Sts2.Core.Logging;
using HarmonyLib;

namespace cakemod.Patches;

public static class PerfectedStrikeChoiceHelper
{
    private static readonly List<Type> AllPerfectCardsExceptStrike = new List<Type>
    {
        typeof(PerfectBash),
        typeof(PerfectPerfected),
        typeof(PerfectBattleTrance),
        typeof(PerfectColossus),
        typeof(PerfectDefendIronclad),
        typeof(PerfectGrandFinale),
        typeof(PerfectHemokinesis),
        typeof(PerfectInfernalBlade),
        typeof(PerfectMindBlast),
        typeof(PerfectPyre),
        typeof(PerfectRage),
        typeof(PerfectRend),
        typeof(PerfectTransfigure),
        typeof(PerfectSwordSage)
    };

    private static MethodInfo _createCardMethodCache;
    private static List<Type> _availablePool = new List<Type>(AllPerfectCardsExceptStrike);

    public static async Task HandlePerfectedStrikeAddedToDeck(PerfectedStrike perfectedStrike)
    {
        // 如果已经选择过，则不执行抽卡
        if (PerfectedStrikePatch.IsSelected(perfectedStrike))
            return;
        
        var localPlayerId = PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform);
        var combatState = perfectedStrike.CombatState;
        
        // 检测打击牌数量，计算抽卡次数
        int strikeCount;
        if (combatState != null)
        {
            strikeCount = perfectedStrike.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(CardTag.Strike));
        }
        else
        {
            // 战斗外从 Player 的 Deck 获取
            var deckPile = PileType.Deck.GetPile(perfectedStrike.Owner);
            strikeCount = deckPile.Cards.Count((CardModel c) => c.Tags.Contains(CardTag.Strike));
        }
        int drawTimes = strikeCount / 3;
        
        if (drawTimes <= 0) return;

        var rng = perfectedStrike.Owner.RunState.Rng.CombatCardSelection;
        
        if (_createCardMethodCache == null)
        {
            _createCardMethodCache = perfectedStrike.Owner.RunState.GetType().GetMethod("CreateCard", new[] { typeof(Player) });
        }

        // 执行 n 次抽卡
        for (int draw = 0; draw < drawTimes; draw++)
        {
            // 为每次选择创建新的 context
            PlayerChoiceContext context;
            if (combatState != null)
            {
                context = new HookPlayerChoiceContext(perfectedStrike, localPlayerId, combatState, GameActionType.Combat);
            }
            else
            {
                context = new BlockingPlayerChoiceContext();
            }
            
            var choices = new List<CardModel>();
            var choiceTypes = new List<Type>();
            var tempPool = new List<Type>(_availablePool);
            
            // 从可用池中随机抽取3张不重复的卡
            for (int i = 0; i < 3; i++)
            {
                Type cardType;
                
                if (tempPool.Count > 0)
                {
                    int index = rng.NextInt(tempPool.Count);
                    cardType = tempPool[index];
                    tempPool.RemoveAt(index);
                }
                else
                {
                    // 池子空了，用 PerfectStrikeIronclad 填充
                    cardType = typeof(PerfectStrikeIronclad);
                }
                
                choiceTypes.Add(cardType);
                var genericMethod = _createCardMethodCache.MakeGenericMethod(cardType);
                var card = (CardModel)genericMethod.Invoke(perfectedStrike.Owner.RunState, new object[] { perfectedStrike.Owner });
                choices.Add(card);
            }

            var selected = await CardSelectHelper.SelectFromChoices(context, choices, perfectedStrike.Owner);

            if (selected != null)
            {
                Type selectedCardType = selected.GetType();
                
                // 只有被选中的卡才从池中移除
                _availablePool.Remove(selectedCardType);
                
                ApplyChoice(perfectedStrike, selectedCardType);
                
                if (perfectedStrike.DeckVersion is PerfectedStrike deckVersion)
                {
                    ApplyChoice(deckVersion, selectedCardType);
                }
            }
        }
        
        // 抽卡完成后重置卡池
        _availablePool = new List<Type>(AllPerfectCardsExceptStrike);
    }

    private static void ApplyChoice(PerfectedStrike card, Type selectedCardType)
    {
        switch (selectedCardType.Name)
        {
            case nameof(PerfectBash):
                PerfectedStrikePatch.IncreaseDamage(card, 2);
                PerfectedStrikePatch.IncreaseVulnerablePower(card, 2);
                break;
            case nameof(PerfectPerfected):
                PerfectedStrikePatch.IncreaseDamage(card, 2);
                PerfectedStrikePatch.IncreaseExtraDamageVar(card, 1);
                break;
            case nameof(PerfectBattleTrance):
                PerfectedStrikePatch.IncreaseCards(card, 2);
                break;
            case nameof(PerfectColossus):
                PerfectedStrikePatch.IncreaseStrengthLoss(card, 3);
                break;
            case nameof(PerfectDefendIronclad):
                PerfectedStrikePatch.IncreaseBlock(card, 8);
                break;
            case nameof(PerfectGrandFinale):
                PerfectedStrikePatch.IncreaseDamage(card, 7);
                PerfectedStrikePatch.IncreaseGrand(card, 1);
                break;
            case nameof(PerfectHemokinesis):
                PerfectedStrikePatch.IncreaseDamage(card, 11);
                PerfectedStrikePatch.IncreaseHpLoss(card, 2);
                break;
            case nameof(PerfectInfernalBlade):
                PerfectedStrikePatch.IncreaseDamage(card, 4);
                PerfectedStrikePatch.IncreaseInfernal(card, 4);
                break;
            case nameof(PerfectMindBlast):
                PerfectedStrikePatch.IncreaseDamage(card, 6);
                PerfectedStrikePatch.SetInnate(card, true);
                break;
            case nameof(PerfectPyre):
                PerfectedStrikePatch.IncreaseEnergy(card, 1);
                break;
            case nameof(PerfectRage):
                PerfectedStrikePatch.IncreaseWeak(card, 2);
                break;
            case nameof(PerfectRend):
                PerfectedStrikePatch.IncreaseDamage(card, 9);
                PerfectedStrikePatch.SetEthereal(card, true);
                break;
            case nameof(PerfectStrikeIronclad):
                PerfectedStrikePatch.IncreaseDamage(card, 5);
                break;
            case nameof(PerfectTransfigure):
                PerfectedStrikePatch.SetExhaust(card, true);
                break;
            case nameof(PerfectSwordSage):
                PerfectedStrikePatch.IncreaseSage(card, 1);
                break;
        }
        
        PerfectedStrikePatch.MarkSelected(card);
    }
}
