using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace cakemod.Scripts
{
    [HarmonyPatch(typeof(PaelsTooth))]
    public static class PaelsToothPatch
    {
        private static ConditionalWeakTable<PaelsTooth, Dictionary<string, PerfectedStrikePatch.CardData>> _savedDataMap = 
            new ConditionalWeakTable<PaelsTooth, Dictionary<string, PerfectedStrikePatch.CardData>>();

        [HarmonyPatch("AfterObtained")]
        [HarmonyPostfix]
        public static void AfterObtainedPostfix(PaelsTooth __instance)
        {
            var savedData = _savedDataMap.GetOrCreateValue(__instance);
            savedData.Clear();
            
            foreach (var serCard in __instance.SerializableCards)
            {
                if (serCard.Id.Entry == "PerfectedStrike")
                {
                    var deckPile = PileType.Deck.GetPile(__instance.Owner);
                    var originalCard = deckPile.Cards.OfType<PerfectedStrike>()
                        .FirstOrDefault(c => c.Id.Entry == serCard.Id.Entry);
                    
                    if (originalCard != null)
                    {
                        var data = PerfectedStrikePatch.GetCardData(originalCard);
                        savedData[serCard.Id.ToString()] = new PerfectedStrikePatch.CardData
                        {
                            CurrentDamage = data.CurrentDamage,
                            CurrentBlock = data.CurrentBlock,
                            CurrentVulnerable = data.CurrentVulnerable,
                            CurrentExtraDamage = data.CurrentExtraDamage,
                            CurrentIsSelected = data.CurrentIsSelected,
                            CurrentHasExhaust = data.CurrentHasExhaust,
                            CurrentInnate = data.CurrentInnate,
                            CurrentEthereal = data.CurrentEthereal,
                            CurrentHpLoss = data.CurrentHpLoss,
                            CurrentCards = data.CurrentCards,
                            CurrentEnergy = data.CurrentEnergy,
                            CurrentStrengthLoss = data.CurrentStrengthLoss,
                            CurrentInfernal = data.CurrentInfernal,
                            CurrentWeak = data.CurrentWeak,
                            CurrentGrand = data.CurrentGrand,
                            CurrentSage = data.CurrentSage
                        };
                    }
                }
            }
        }

        [HarmonyPatch("AfterCombatEnd")]
        [HarmonyPostfix]
        public static void AfterCombatEndPostfix(PaelsTooth __instance)
        {
            if (!_savedDataMap.TryGetValue(__instance, out var savedData))
                return;

            var deckPile = PileType.Deck.GetPile(__instance.Owner);
            var perfectedStrikes = deckPile.Cards.OfType<PerfectedStrike>().ToList();
            
            foreach (var strike in perfectedStrikes)
            {
                string cardKey = strike.Id.ToString();
                if (savedData.TryGetValue(cardKey, out var data))
                {
                    var currentData = PerfectedStrikePatch.GetCardData(strike);
                    currentData.CurrentDamage = data.CurrentDamage;
                    currentData.CurrentBlock = data.CurrentBlock;
                    currentData.CurrentVulnerable = data.CurrentVulnerable;
                    currentData.CurrentExtraDamage = data.CurrentExtraDamage;
                    currentData.CurrentIsSelected = data.CurrentIsSelected;
                    currentData.CurrentHasExhaust = data.CurrentHasExhaust;
                    currentData.CurrentInnate = data.CurrentInnate;
                    currentData.CurrentEthereal = data.CurrentEthereal;
                    currentData.CurrentHpLoss = data.CurrentHpLoss;
                    currentData.CurrentCards = data.CurrentCards;
                    currentData.CurrentEnergy = data.CurrentEnergy;
                    currentData.CurrentStrengthLoss = data.CurrentStrengthLoss;
                    currentData.CurrentInfernal = data.CurrentInfernal;
                    currentData.CurrentWeak = data.CurrentWeak;
                    currentData.CurrentGrand = data.CurrentGrand;
                    currentData.CurrentSage = data.CurrentSage;

                    strike.DynamicVars.CalculationBase.BaseValue = data.CurrentDamage;
                    strike.DynamicVars.Block.BaseValue = data.CurrentBlock;
                    strike.DynamicVars.Vulnerable.BaseValue = data.CurrentVulnerable;
                    strike.DynamicVars.ExtraDamage.BaseValue = data.CurrentExtraDamage;
                    strike.DynamicVars["IsSelected"].BaseValue = data.CurrentIsSelected;
                    strike.DynamicVars["HasExhaust"].BaseValue = data.CurrentHasExhaust;
                    strike.DynamicVars["Innate"].BaseValue = data.CurrentInnate;
                    strike.DynamicVars["Ethereal"].BaseValue = data.CurrentEthereal;
                    strike.DynamicVars.HpLoss.BaseValue = data.CurrentHpLoss;
                    strike.DynamicVars.Cards.BaseValue = data.CurrentCards;
                    strike.DynamicVars.Energy.BaseValue = data.CurrentEnergy;
                    strike.DynamicVars["StrengthLoss"].BaseValue = data.CurrentStrengthLoss;
                    strike.DynamicVars["Infernal"].BaseValue = data.CurrentInfernal;
                    strike.DynamicVars.Weak.BaseValue = data.CurrentWeak;
                    strike.DynamicVars["Grand"].BaseValue = data.CurrentGrand;
                    strike.DynamicVars["Sage"].BaseValue = data.CurrentSage;

                    if (data.CurrentHasExhaust > 0)
                    {
                        strike.AddKeyword(CardKeyword.Exhaust);
                        strike.BaseReplayCount = 1;
                    }
                    if (data.CurrentInnate > 0) strike.AddKeyword(CardKeyword.Innate);
                    if (data.CurrentEthereal > 0) strike.AddKeyword(CardKeyword.Ethereal);
                    if (data.CurrentSage > 0) strike.EnergyCost.UpgradeBy(1);

                    savedData.Remove(cardKey);
                }
            }
        }
    }
}
