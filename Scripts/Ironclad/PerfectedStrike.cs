using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.CardSelection;

namespace cakemod.Scripts
{
    [HarmonyPatch(typeof(PerfectedStrike))]
    public static class PerfectedStrikePatch
    {

        private static ConditionalWeakTable<PerfectedStrike, CardData> _cardDataMap = new ConditionalWeakTable<PerfectedStrike, CardData>();

        public class CardData
        {
            public int CurrentDamage = 10;
            public int CurrentBlock = 0;
            public int CurrentVulnerable = 0;
            public int CurrentExtraDamage = 0;
            public int CurrentIsSelected = 0;
            public int CurrentHasExhaust = 0;
            public int CurrentInnate = 0;
            public int CurrentEthereal = 0;
            public int CurrentHpLoss = 0;
            public int CurrentCards = 0;
            public int CurrentEnergy = 0;
            public int CurrentStrengthLoss = 0;
            public int CurrentInfernal = 0;
            public int CurrentWeak = 0;
            public int CurrentGrand = 0;
            public int CurrentSage = 0;
        }

        public static CardData GetCardData(PerfectedStrike card)
        {
            return _cardDataMap.GetOrCreateValue(card);
        }


        [HarmonyPatch(typeof(SavedProperties), nameof(SavedProperties.FromInternal))]
        [HarmonyPostfix]
        public static void SaveCustomData(object model, ref SavedProperties __result)
        {
            // 如果当前正在保存的对象是 PerfectedStrike
            if (model is PerfectedStrike card)
            {
                var data = GetCardData(card);

                // 确保 __result 和 ints 列表不为空
                if (__result == null) __result = new SavedProperties();
                if (__result.ints == null) __result.ints = new List<SavedProperties.SavedProperty<int>>();

                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentDamage", data.CurrentDamage));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentBlock", data.CurrentBlock));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentVulnerable", data.CurrentVulnerable));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentExtraDamage", data.CurrentExtraDamage));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentIsSelected", data.CurrentIsSelected));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentHasExhaust", data.CurrentHasExhaust));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentInnate", data.CurrentInnate));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentEthereal", data.CurrentEthereal));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentHpLoss", data.CurrentHpLoss));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentCards", data.CurrentCards));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentEnergy", data.CurrentEnergy));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentStrengthLoss", data.CurrentStrengthLoss));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentInfernal", data.CurrentInfernal));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentWeak", data.CurrentWeak));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentGrand", data.CurrentGrand));
                __result.ints.Add(new SavedProperties.SavedProperty<int>("Mod_CurrentSage", data.CurrentSage));
            }
        }


        [HarmonyPatch(typeof(SavedProperties), nameof(SavedProperties.FillInternal))]
        [HarmonyPostfix]
        public static void LoadCustomData(SavedProperties __instance, object model)
        {
            // 如果当前正在读取的对象是 PerfectedStrike，并且存档里有 int 数据
            if (model is PerfectedStrike card && __instance.ints != null)
            {
                var data = GetCardData(card);

                // 遍历存档中的 int 属性，寻找我们之前存进去的 Key
                foreach (var prop in __instance.ints)
                {
                    switch (prop.name)
                    {
                        case "Mod_CurrentDamage":
                            data.CurrentDamage = prop.value;
                            card.DynamicVars.CalculationBase.BaseValue = data.CurrentDamage;
                            break;
                        case "Mod_CurrentBlock":
                            data.CurrentBlock = prop.value;
                            card.DynamicVars.Block.BaseValue = data.CurrentBlock;
                            break;
                        case "Mod_CurrentVulnerable":
                            data.CurrentVulnerable = prop.value;
                            card.DynamicVars.Vulnerable.BaseValue = data.CurrentVulnerable;
                            break;
                        case "Mod_CurrentExtraDamage":
                            data.CurrentExtraDamage = prop.value;
                            card.DynamicVars.ExtraDamage.BaseValue = data.CurrentExtraDamage;
                            break;
                        case "Mod_CurrentIsSelected":
                            data.CurrentIsSelected = prop.value;
                            card.DynamicVars["IsSelected"].BaseValue = data.CurrentIsSelected;
                            break;
                        case "Mod_CurrentHasExhaust":
                            data.CurrentHasExhaust = prop.value;
                            card.DynamicVars["HasExhaust"].BaseValue = data.CurrentHasExhaust;
                            if (prop.value > 0)
                            {
                            card.AddKeyword(CardKeyword.Exhaust);
                            card.BaseReplayCount = 1;
                            } 
                            break;
                        case "Mod_CurrentInnate":
                            data.CurrentInnate = prop.value;
                            card.DynamicVars["Innate"].BaseValue = data.CurrentInnate;
                            if (prop.value > 0) card.AddKeyword(CardKeyword.Innate);
                            break;
                        case "Mod_CurrentEthereal":
                            data.CurrentEthereal = prop.value;
                            card.DynamicVars["Ethereal"].BaseValue = data.CurrentEthereal;
                            if (prop.value > 0) card.AddKeyword(CardKeyword.Ethereal);
                            break;
                        case "Mod_CurrentHpLoss":
                            data.CurrentHpLoss = prop.value;
                            card.DynamicVars.HpLoss.BaseValue = data.CurrentHpLoss;
                            break;
                        case "Mod_CurrentCards":
                            data.CurrentCards = prop.value;
                            card.DynamicVars.Cards.BaseValue = data.CurrentCards;
                            break;
                        case "Mod_CurrentEnergy":
                            data.CurrentEnergy = prop.value;
                            card.DynamicVars.Energy.BaseValue = data.CurrentEnergy;
                            break;
                        case "Mod_CurrentStrengthLoss":
                            data.CurrentStrengthLoss = prop.value;
                            card.DynamicVars["StrengthLoss"].BaseValue = data.CurrentStrengthLoss;
                            break;
                        case "Mod_CurrentInfernal":
                            data.CurrentInfernal = prop.value;
                            card.DynamicVars["Infernal"].BaseValue = data.CurrentInfernal;
                            break;
                        case "Mod_CurrentWeak":
                            data.CurrentWeak = prop.value;
                            card.DynamicVars.Weak.BaseValue = data.CurrentWeak;
                            break;
                        case "Mod_CurrentGrand":
                            data.CurrentGrand = prop.value;
                            card.DynamicVars["Grand"].BaseValue = data.CurrentGrand;
                            break;
                        case "Mod_CurrentSage":
                            data.CurrentSage = prop.value;
                            card.DynamicVars["Sage"].BaseValue = data.CurrentSage;
                            if (prop.value > 0) card.EnergyCost.UpgradeBy(1);
                            break;
                    }
                }
            }
        }


        public static void IncreaseDamage(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentDamage += (int)amount;
            card.DynamicVars.CalculationBase.BaseValue = data.CurrentDamage;
        }

        public static void IncreaseBlock(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentBlock += (int)amount;
            card.DynamicVars.Block.BaseValue = data.CurrentBlock;
        }

        public static void IncreaseVulnerablePower(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentVulnerable += (int)amount;
            card.DynamicVars.Vulnerable.BaseValue = data.CurrentVulnerable;
        }

        public static void IncreaseExtraDamageVar(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentExtraDamage += (int)amount;
            card.DynamicVars.ExtraDamage.BaseValue = data.CurrentExtraDamage;
        }

        public static void SetExhaust(PerfectedStrike card, bool hasExhaust)
        {
            var data = GetCardData(card);
            data.CurrentHasExhaust = hasExhaust ? 1 : 0;
            card.DynamicVars["HasExhaust"].BaseValue = data.CurrentHasExhaust;
            if (hasExhaust) {
            card.AddKeyword(CardKeyword.Exhaust);
            card.BaseReplayCount = 1;
            }
        }


        public static void MarkSelected(PerfectedStrike card)
        {
            var data = GetCardData(card);
            data.CurrentIsSelected = 1;
            card.DynamicVars["IsSelected"].BaseValue = data.CurrentIsSelected;
        }
        
        
        public static bool IsSelected(PerfectedStrike card)
        {
            var data = GetCardData(card);
            return data.CurrentIsSelected > 0;
        }

        public static void SetInnate(PerfectedStrike card, bool hasInnate)
        {
            var data = GetCardData(card);
            data.CurrentInnate = hasInnate ? 1 : 0;
            card.DynamicVars["Innate"].BaseValue = data.CurrentInnate;
            if (hasInnate) card.AddKeyword(CardKeyword.Innate);
        }

        public static void SetEthereal(PerfectedStrike card, bool hasEthereal)
        {
            var data = GetCardData(card);
            data.CurrentEthereal = hasEthereal ? 1 : 0;
            card.DynamicVars["Ethereal"].BaseValue = data.CurrentEthereal;
            if (hasEthereal) card.AddKeyword(CardKeyword.Ethereal);
        }

        public static void IncreaseHpLoss(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentHpLoss += (int)amount;
            card.DynamicVars.HpLoss.BaseValue = data.CurrentHpLoss;
        }

        public static void IncreaseCards(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentCards += (int)amount;
            card.DynamicVars.Cards.BaseValue = data.CurrentCards;
        }

        public static void IncreaseEnergy(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentEnergy += (int)amount;
            card.DynamicVars.Energy.BaseValue = data.CurrentEnergy;
        }

        public static void IncreaseStrengthLoss(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentStrengthLoss += (int)amount;
            card.DynamicVars["StrengthLoss"].BaseValue = data.CurrentStrengthLoss;
        }

        public static void IncreaseInfernal(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentInfernal += (int)amount;
            card.DynamicVars["Infernal"].BaseValue = data.CurrentInfernal;
        }

        public static void IncreaseWeak(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentWeak += (int)amount;
            card.DynamicVars.Weak.BaseValue = data.CurrentWeak;
        }

        public static void IncreaseGrand(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentGrand += (int)amount;
            card.DynamicVars["Grand"].BaseValue = data.CurrentGrand;
        }

        public static void IncreaseSage(PerfectedStrike card, decimal amount)
        {
            var data = GetCardData(card);
            data.CurrentSage += (int)amount;
            card.EnergyCost.UpgradeBy(1);
            card.DynamicVars["Sage"].BaseValue = data.CurrentSage;
        }



        [HarmonyPatch("CanonicalVars", MethodType.Getter)]
        [HarmonyPostfix]
        public static void CanonicalVarsPostfix(PerfectedStrike __instance, ref IEnumerable<DynamicVar> __result)
        {
            var data = GetCardData(__instance);
            __result = new DynamicVar[]
            {
                new CalculationBaseVar(data.CurrentDamage),
                new BlockVar(data.CurrentBlock, ValueProp.Move),
                new PowerVar<VulnerablePower>(data.CurrentVulnerable),
                new ExtraDamageVar(data.CurrentExtraDamage),
                new IntVar("IsSelected", data.CurrentIsSelected),
                new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => 
                    card.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(CardTag.Strike))),
                new IntVar("HasExhaust", data.CurrentHasExhaust),
                new IntVar("Innate", data.CurrentInnate),
                new IntVar("Ethereal", data.CurrentEthereal),
                new HpLossVar(data.CurrentHpLoss),
                new CardsVar(data.CurrentCards),
                new EnergyVar(data.CurrentEnergy),
                new DynamicVar("StrengthLoss", data.CurrentStrengthLoss),
                new DynamicVar("Infernal", data.CurrentInfernal),
                new PowerVar<WeakPower>(data.CurrentWeak),
                new IntVar("Grand", data.CurrentGrand),
                new IntVar("Sage", data.CurrentSage)
            };
        }

        [HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
        public static class PerfectedStrikeKeywordPatch
        {
            [HarmonyPostfix]
            public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
            {
                if (__instance is PerfectedStrike card)
                {
                    __result = __result ?? Enumerable.Empty<CardKeyword>();
                    
                    if (card.DynamicVars.TryGetValue("HasExhaust", out var exhaustVar) && exhaustVar.IntValue > 0)
                    {
                        if (!__result.Contains(CardKeyword.Exhaust))
                        {
                            __result = __result.Append(CardKeyword.Exhaust);
                        }
                    }
                    
                    if (card.DynamicVars.TryGetValue("Innate", out var innateVar) && innateVar.IntValue > 0)
                    {
                        if (!__result.Contains(CardKeyword.Innate))
                        {
                            __result = __result.Append(CardKeyword.Innate);
                        }
                    }
                    
                    if (card.DynamicVars.TryGetValue("Ethereal", out var etherealVar) && etherealVar.IntValue > 0)
                    {
                        if (!__result.Contains(CardKeyword.Ethereal))
                        {
                            __result = __result.Append(CardKeyword.Ethereal);
                        }
                    }
                }
            }
        }

        [HarmonyPatch("OnPlay")]
        [HarmonyPrefix]
        public static bool OnPlayPrefix(PerfectedStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
        {
            __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
            return false;
        }

        private static async Task PatchedOnPlay(PerfectedStrike card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            
            // PerfectHemokinesis: 失去生命
            int hpLoss = card.DynamicVars.HpLoss.IntValue;
            if (hpLoss > 0)
            {
                await CreatureCmd.Damage(choiceContext, card.Owner.Creature, hpLoss, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
            }
            
            // Grand: 2倍伤害（当抽牌堆为空时）
            int grandValue = card.DynamicVars["Grand"].IntValue;
            CardPile drawPile = PileType.Draw.GetPile(card.Owner);
            bool isDrawPileEmpty = drawPile.Cards.Count == 0;
            
            if (grandValue > 0 && isDrawPileEmpty)
            {
                var damageVar = card.DynamicVars.CalculatedDamage.WithMultiplier((CardModel _, Creature? __) => 2m);
                await DamageCmd.Attack((CalculatedDamageVar)damageVar).FromCard(card).Targeting(cardPlay.Target)
                    .WithHitFx(null, null, "heavy_attack.mp3")
                    .WithHitCount(1+card.DynamicVars["Sage"].IntValue)
                    .Execute(choiceContext);
            }
            else
            {
                await DamageCmd.Attack(card.DynamicVars.CalculatedDamage).FromCard(card).Targeting(cardPlay.Target)
                    .WithHitFx(null, null, "heavy_attack.mp3")
                    .WithHitCount(1+card.DynamicVars["Sage"].IntValue)
                    .Execute(choiceContext);
            }


            // Infernal: 检测Infernal数量并让玩家选择卡牌
            int infernalCount = card.DynamicVars["Infernal"].IntValue;
            if (infernalCount > 0)
            {
                LocString prompt = Traverse.Create(card).Property("SelectionScreenPrompt").GetValue<LocString>();
                var selectedCards = (await CustomCardSelectCmd.FromDraw(
                    prefs: new CardSelectorPrefs(prompt, 0, infernalCount), 
                    context: choiceContext, 
                    player: card.Owner, 
                    filter: null, 
                    source: card,
                    shuffle: true
                )).ToList();

                foreach (CardModel item in selectedCards)
                {
                    await CardPileCmd.Add(item, PileType.Discard);
                }
            }

            // PerfectBash: 易伤
            int vulnerableAmount = card.DynamicVars.Vulnerable.IntValue;
            if (vulnerableAmount > 0)
            {
                await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, vulnerableAmount, card.Owner.Creature, card);
            }
            
            // PerfectRage: 虚弱
            int weakAmount = card.DynamicVars.Weak.IntValue;
            if (weakAmount > 0)
            {
                await PowerCmd.Apply<WeakPower>(cardPlay.Target, weakAmount, card.Owner.Creature, card);
            }
            
            // PerfectDefendIronclad: 格挡
            int blockAmount = card.DynamicVars.Block.IntValue;
            if (blockAmount > 0)
            {
                await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, cardPlay);
            }
            
            // PerfectBattleTrance: 抽牌
            int drawAmount = card.DynamicVars.Cards.IntValue;
            if (drawAmount > 0)
            {
                await CardPileCmd.Draw(choiceContext, drawAmount, card.Owner);
            }
            
            // PerfectPyre: 回复能量
            int energyAmount = card.DynamicVars.Energy.IntValue;
            if (energyAmount > 0)
            {
                await PlayerCmd.GainEnergy(energyAmount, card.Owner);
            }
            
            // PerfectColossus:
            int strengthLoss = card.DynamicVars["StrengthLoss"].IntValue;
            if (strengthLoss > 0)
            {
                var enemies = card.CombatState.Enemies.Where(e => !e.IsDead);
                foreach (var enemy in enemies)
                {
                    await PowerCmd.Apply<PiercingWailPower>(enemy, strengthLoss, card.Owner.Creature, card);
                }
            }
            
            // Transfigure: HasExhaust时添加2张卡到抽牌堆
            int hasExhaust = card.DynamicVars["HasExhaust"].IntValue;
            if (hasExhaust > 0)
            {
            CardModel thunder1 = card.CombatState.CreateCard<Thunder>(card.Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(thunder1, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
            CardModel thunder2 = card.CombatState.CreateCard<Thunder>(card.Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(thunder2, PileType.Draw, addedByPlayer: true, CardPilePosition.Random));
            
            }
        }

        [HarmonyPatch("OnUpgrade")]
        [HarmonyPrefix]
        public static bool OnUpgradePrefix(PerfectedStrike __instance)
        {
            __instance.DynamicVars.CalculationBase.UpgradeValueBy(3m);
            return false;
        }

        [HarmonyPatch(typeof(CardModel), "ShouldGlowGoldInternal", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool ShouldGlowGoldInternalPrefix(CardModel __instance, ref bool __result)
        {
            if (__instance is PerfectedStrike card)
            {
                __result = PileType.Draw.GetPile(card.Owner).Cards.Count == 0 &&  card.DynamicVars["Grand"].IntValue > 0;
                return false;
            }
            return true;
        }

    }
}
