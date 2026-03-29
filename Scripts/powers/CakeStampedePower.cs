using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Extensions;
using cakemod.Scripts.function;

namespace cakemod.Scripts;

public sealed class CakeStampedePower : CakePowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] 
	{ 
		HoverTipFactory.FromCard<Thunder>()
	};

	public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player.Creature != base.Owner)
		{
			return;
		}
		Flash();

		CardPile drawPile = PileType.Draw.GetPile(base.Owner.Player);
		var topCards = drawPile.Cards.Take(5).ToList();

		if (!topCards.Any())
		{
		return;
		}

		LocString prompt = Traverse.Create(this).Property("SelectionScreenPrompt").GetValue<LocString>();
		await CustomCardSelectCmd.FromDraw(
			context: choiceContext,
			player: base.Owner.Player,
			prefs: new CardSelectorPrefs(prompt, 0, 0),
			filter: null,
			source: this,
			topCount: 5
		);

		var choice1 = combatState.CreateCard<Choice1>(base.Owner.Player);
		var choice2 = combatState.CreateCard<Choice2>(base.Owner.Player);
		var choices = new List<CardModel> { choice1, choice2 };
		
		var selectedChoice = await CardSelectHelper.SelectFromChoices(choiceContext, choices, base.Owner.Player);

		if (selectedChoice is Choice1)
		{
				int maxCost = topCards.Max(c => c.EnergyCost.GetWithModifiers(CostModifiers.All));
				var highestCostCards = topCards.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.All) == maxCost).ToList();
				var highestCostCard = highestCostCards.UnstableShuffle(base.Owner.Player.RunState.Rng.CombatCardSelection).First();
				await CardCmd.AutoPlay(choiceContext, highestCostCard, null);
				await CardCmd.AutoPlay(choiceContext, highestCostCard, null);

			for (int i = 0; i < 3; i++)
			{
				CardModel thunder = combatState.CreateCard<Thunder>(base.Owner.Player);
				await CardPileCmd.AddGeneratedCardToCombat(thunder, PileType.Draw, addedByPlayer: true,CardPilePosition.Random);
			}
		}
		else if (selectedChoice is Choice2)
		{
			foreach (var card in topCards)
			{
				await CardPileCmd.Add(card, PileType.Discard);
			}
		}
	}
}
