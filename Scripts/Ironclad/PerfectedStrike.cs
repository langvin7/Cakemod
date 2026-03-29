using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Runtime.CompilerServices;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;


namespace cakemod.Scripts
{
    public static class PerfectedStrikePatch
    {
        private static readonly ConditionalWeakTable<PerfectedStrike, ExtraData> extraDataMap = new ConditionalWeakTable<PerfectedStrike, ExtraData>();

        private class ExtraData
        {
            public decimal BonusDamage = 0m;
            public decimal BonusBlock = 0m;
            public bool isSelected = false;
        }

        private static ExtraData GetExtraData(PerfectedStrike card)
        {
            return extraDataMap.GetOrCreateValue(card);
        }

        public static void IncreaseDamage(PerfectedStrike card, decimal amount)
        {
            var data = GetExtraData(card);
            data.BonusDamage += amount;
            
            // Recompute calculation base var or extra damage var
            var baseVar = card.DynamicVars.CalculationBase;
            if (baseVar != null)
            {
               baseVar.BaseValue += amount;
            }
        }

        public static void IncreaseBlock(PerfectedStrike card, decimal amount)
        {
            var data = GetExtraData(card);
            data.BonusBlock += amount;
            
            // Add block to card
            var blockVar = card.DynamicVars.Block;
            if (blockVar != null)
            {
               blockVar.BaseValue += amount;
            }
        }

        public static void MarkSelected(PerfectedStrike card)
        {
            var data = GetExtraData(card);
            data.isSelected = true;
        }

        public static bool IsSelected(PerfectedStrike card)
        {
            var data = GetExtraData(card);
            return data.isSelected;
        }

        [HarmonyPatch(typeof(PerfectedStrike), "CanonicalVars", MethodType.Getter)]
        public static class PerfectedStrikeCanonicalVarsPatch
        {
            private static readonly DynamicVar[] ModifiedVars = [
                new BlockVar(0m, ValueProp.Move),
                new CalculatedVar("IsSelected")
                .WithMultiplier((card, _) => (card is PerfectedStrike ps) ? (IsSelected(ps) ? 1 : 0) : 0),
                new CalculationExtraVar(0m),
                new PowerVar<WeakPower>("SappingWeak", 2m),
		        new PowerVar<VulnerablePower>("SappingVulnerable", 2m)
            ];

            [HarmonyPostfix]
            public static void Postfix(ref IEnumerable<DynamicVar> __result)
            {
                var list = __result.ToList();
                list.AddRange(ModifiedVars);
                __result = list;
            }
        }


        [HarmonyPatch(typeof(PerfectedStrike), "OnPlay")]
        [HarmonyPostfix]
        public static void PostfixOnPlay(PerfectedStrike __instance, PlayerChoiceContext choiceContext, ref Task __result)
        {
            var data = GetExtraData(__instance);
            if (data.BonusBlock > 0)
            {
                Task original = __result;
                __result = RunBonusBlock(__instance, choiceContext, original, data.BonusBlock);
            }
        }

        private static async Task RunBonusBlock(PerfectedStrike card, PlayerChoiceContext choiceContext, Task original, decimal blockAmount)
        {
            await original;
            await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, null);
        }
    }
}
