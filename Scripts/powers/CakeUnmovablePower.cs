using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;

using BaseLib.Abstracts;
using cakemod.Scripts.function;

namespace cakemod.Scripts;

public sealed class CakeUnmovablePower : CakePowerModel
{

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => false;

	// Use DisplayAmount to visually decrease the counter without losing the actual base Amount.
	public override int DisplayAmount
	{
		get
		{
			if (CombatManager.Instance == null || CombatManager.Instance.History == null || base.Owner == null)
			{
				return base.Amount;
			}
			
			int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => 
				e.Actor == base.Owner && 
				e.CardPlay.IsFirstInSeries && 
			(e.CardPlay.Card.Rarity == CardRarity.Common || e.CardPlay.Card.Rarity == CardRarity.Basic )&& 
				e.CardPlay.Card.Type == CardType.Skill && 
				e.HappenedThisTurn(base.CombatState));

			int remaining = base.Amount - num;
			return remaining < 0 ? 0 : remaining;
		}
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return playCount;
		}

		if ((card.Rarity != CardRarity.Common && card.Rarity != CardRarity.Basic) || card.Type != CardType.Skill)
		{
			return playCount;
		}

		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => 
			e.Actor == base.Owner && 
			e.CardPlay.IsFirstInSeries && 
			e.CardPlay.Card.Rarity == CardRarity.Common && 
			e.CardPlay.Card.Type == CardType.Skill && 
			e.HappenedThisTurn(base.CombatState));

		if (num >= base.Amount)
		{
			return playCount;
		}
		return playCount + 1;
	}

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		if ((card.Rarity == CardRarity.Common||card.Rarity == CardRarity.Basic) && card.Type == CardType.Skill)
		{
			Flash();
		}
		return Task.CompletedTask;
	}
}
