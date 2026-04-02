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
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace cakemod.Scripts
{
    [HarmonyPatch(typeof(PerfectedStrike))]
    public static class PerfectedStrikePatch
    {

        private static ConditionalWeakTable<PerfectedStrike, CardData> _cardDataMap = new ConditionalWeakTable<PerfectedStrike, CardData>();
        private static bool _isCloningPerfectedStrike = false;
        public static bool IsCloningPerfectedStrike => _isCloningPerfectedStrike;

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

        /// <summary>
        /// 将 CardData 中的所有自定义数据重新同步到卡牌的 DynamicVars 和关键字上。
        /// 用于 Downgrade、Clone、Load 等场景，防止数据被覆盖。
        /// </summary>
        public static void ReloadFromCardData(PerfectedStrike card, CardData data)
        {
            card.DynamicVars.CalculationBase.BaseValue = data.CurrentDamage;
            card.DynamicVars.Block.BaseValue = data.CurrentBlock;
            card.DynamicVars.Vulnerable.BaseValue = data.CurrentVulnerable;
            card.DynamicVars.ExtraDamage.BaseValue = data.CurrentExtraDamage;
            card.DynamicVars["IsSelected"].BaseValue = data.CurrentIsSelected;
            card.DynamicVars["HasExhaust"].BaseValue = data.CurrentHasExhaust;
            card.DynamicVars["Innate"].BaseValue = data.CurrentInnate;
            card.DynamicVars["Ethereal"].BaseValue = data.CurrentEthereal;
            card.DynamicVars.HpLoss.BaseValue = data.CurrentHpLoss;
            card.DynamicVars.Cards.BaseValue = data.CurrentCards;
            card.DynamicVars.Energy.BaseValue = data.CurrentEnergy;
            card.DynamicVars["StrengthLoss"].BaseValue = data.CurrentStrengthLoss;
            card.DynamicVars["Infernal"].BaseValue = data.CurrentInfernal;
            card.DynamicVars.Weak.BaseValue = data.CurrentWeak;
            card.DynamicVars["Grand"].BaseValue = data.CurrentGrand;
            card.DynamicVars["Sage"].BaseValue = data.CurrentSage;

            if (data.CurrentHasExhaust > 0)
            {
                card.AddKeyword(CardKeyword.Exhaust);
                card.BaseReplayCount = 1;
            }
            if (data.CurrentInnate > 0)
            {
                card.AddKeyword(CardKeyword.Innate);
            }
            if (data.CurrentEthereal > 0)
            {
                card.AddKeyword(CardKeyword.Ethereal);
            }
        }

        /// <summary>
        /// 将一个 CardData 的所有字段复制到另一个 CardData（用于克隆场景）。
        /// </summary>
        public static void CopyCardData(CardData source, CardData dest)
        {
            dest.CurrentDamage = source.CurrentDamage;
            dest.CurrentBlock = source.CurrentBlock;
            dest.CurrentVulnerable = source.CurrentVulnerable;
            dest.CurrentExtraDamage = source.CurrentExtraDamage;
            dest.CurrentIsSelected = source.CurrentIsSelected;
            dest.CurrentHasExhaust = source.CurrentHasExhaust;
            dest.CurrentInnate = source.CurrentInnate;
            dest.CurrentEthereal = source.CurrentEthereal;
            dest.CurrentHpLoss = source.CurrentHpLoss;
            dest.CurrentCards = source.CurrentCards;
            dest.CurrentEnergy = source.CurrentEnergy;
            dest.CurrentStrengthLoss = source.CurrentStrengthLoss;
            dest.CurrentInfernal = source.CurrentInfernal;
            dest.CurrentWeak = source.CurrentWeak;
            dest.CurrentGrand = source.CurrentGrand;
            dest.CurrentSage = source.CurrentSage;
        }

        [HarmonyPatch(typeof(CardModel), nameof(CardModel.DowngradeInternal))]
        [HarmonyPostfix]
        public static void DowngradeInternalPostfix(CardModel __instance)
        {
            if (__instance is PerfectedStrike card)
            {
                var data = GetCardData(card);
                ReloadFromCardData(card, data);
            }
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
            if (model is PerfectedStrike card && __instance.ints != null)
            {
                var data = GetCardData(card);

                foreach (var prop in __instance.ints)
                {
                    switch (prop.name)
                    {
                        case "Mod_CurrentDamage": data.CurrentDamage = prop.value; break;
                        case "Mod_CurrentBlock": data.CurrentBlock = prop.value; break;
                        case "Mod_CurrentVulnerable": data.CurrentVulnerable = prop.value; break;
                        case "Mod_CurrentExtraDamage": data.CurrentExtraDamage = prop.value; break;
                        case "Mod_CurrentIsSelected": data.CurrentIsSelected = prop.value; break;
                        case "Mod_CurrentHasExhaust": data.CurrentHasExhaust = prop.value; break;
                        case "Mod_CurrentInnate": data.CurrentInnate = prop.value; break;
                        case "Mod_CurrentEthereal": data.CurrentEthereal = prop.value; break;
                        case "Mod_CurrentHpLoss": data.CurrentHpLoss = prop.value; break;
                        case "Mod_CurrentCards": data.CurrentCards = prop.value; break;
                        case "Mod_CurrentEnergy": data.CurrentEnergy = prop.value; break;
                        case "Mod_CurrentStrengthLoss": data.CurrentStrengthLoss = prop.value; break;
                        case "Mod_CurrentInfernal": data.CurrentInfernal = prop.value; break;
                        case "Mod_CurrentWeak": data.CurrentWeak = prop.value; break;
                        case "Mod_CurrentGrand": data.CurrentGrand = prop.value; break;
                        case "Mod_CurrentSage": data.CurrentSage = prop.value; break;
                    }
                }

                ReloadFromCardData(card, data);
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



        [HarmonyPatch(typeof(CardModel), nameof(CardModel.CreateClone))]
        [HarmonyPrefix]
        public static void CreateClonePrefix(CardModel __instance)
        {
            if (__instance is PerfectedStrike)
            {
                _isCloningPerfectedStrike = true;
            }
        }

        [HarmonyPatch(typeof(CardModel), nameof(CardModel.CreateClone))]
        [HarmonyPostfix]
        public static void CreateClonePostfix(CardModel __instance, CardModel __result)
        {
            if (__instance is PerfectedStrike originalCard && __result is PerfectedStrike clonedCard)
            {
                var originalData = GetCardData(originalCard);
                var clonedData = GetCardData(clonedCard);
                CopyCardData(originalData, clonedData);
                ReloadFromCardData(clonedCard, clonedData);
            }
            _isCloningPerfectedStrike = false;
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.CloneCard))]
        [HarmonyPrefix]
        public static void RunStateCloneCardPrefix(CardModel mutableCard)
        {
            if (mutableCard is PerfectedStrike)
            {
                _isCloningPerfectedStrike = true;
            }
        }

        [HarmonyPatch(typeof(RunState), nameof(RunState.CloneCard))]
        [HarmonyPostfix]
        public static void RunStateCloneCardPostfix(CardModel mutableCard, CardModel __result)
        {
            if (mutableCard is PerfectedStrike originalCard && __result is PerfectedStrike clonedCard)
            {
                var originalData = GetCardData(originalCard);
                var clonedData = GetCardData(clonedCard);
                CopyCardData(originalData, clonedData);
                ReloadFromCardData(clonedCard, clonedData);
            }
            _isCloningPerfectedStrike = false;
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
            

            await DamageCmd.Attack(card.DynamicVars.CalculatedDamage).FromCard(card).Targeting(cardPlay.Target)
                    .WithHitFx(null, null, "heavy_attack.mp3")
                    .WithHitCount(1+card.DynamicVars["Sage"].IntValue)
                    .Execute(choiceContext);


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

        [HarmonyPatch(typeof(AbstractModel), "ModifyDamageMultiplicative")]
        [HarmonyPostfix]
        public static void ModifyDamageMultiplicativePostfix(CardModel __instance, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ref decimal __result)
        {
            if (__instance is PerfectedStrike card && cardSource == __instance && card.DynamicVars["Grand"].IntValue > 0)
            {
                CardPile drawPile = PileType.Draw.GetPile(card.Owner);
                if (drawPile.Cards.Count == 0)
                {
                    __result *= 2m;
                }
            }
        }

    }
}
