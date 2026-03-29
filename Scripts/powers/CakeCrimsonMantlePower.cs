using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Utils;
using cakemod.Scripts.function;

namespace cakemod.Scripts;
public sealed class CakeCrimsonMantlePower : CakePowerModel
{

	private class Data
    {
        public readonly Dictionary<CardModel, int> playedCards = new Dictionary<CardModel, int>();
    }

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner)
        {
            return Task.CompletedTask;
        }

        if (base.CombatState.CurrentSide != base.Owner.Side)
        {
            return Task.CompletedTask;
        }

        GetInternalData<Data>().playedCards.Add(cardPlay.Card, 0);
        return Task.CompletedTask;
    }
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner && result.UnblockedDamage > 0 && base.CombatState.CurrentSide == base.Owner.Side)
        {
            if (cardSource == null || !GetInternalData<Data>().playedCards.ContainsKey(cardSource))
            {
                        await CreatureCmd.Heal(base.Owner,1m);
            }
            else
            {
                GetInternalData<Data>().playedCards[cardSource] += base.Amount;
            }
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && GetInternalData<Data>().playedCards.Remove(cardPlay.Card, out var value))
        {
            if(value>0)
            {
                await CreatureCmd.Heal(base.Owner,value);
            }
        }
    }

}
