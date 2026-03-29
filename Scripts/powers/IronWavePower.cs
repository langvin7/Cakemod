using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;

using BaseLib.Abstracts;
using BaseLib.Utils;
using cakemod.Scripts.function;


namespace cakemod.Scripts;

public sealed class IronWavePower : CakePowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			int num = base.Owner.Player.PlayerCombatState.Energy;
			if (num >= 1)
			{
				Flash();
				await CreatureCmd.Damage(choiceContext,base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner);
				await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
			}
		}
	}

		public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Enemy)
		{
			if (base.Owner.Side != CombatSide.Enemy && base.Owner.HasPower<CakeJuggernautPower>())
			{
				
				await PowerCmd.ModifyAmount(this, 1, null, null);
			}
		}
	}
}
