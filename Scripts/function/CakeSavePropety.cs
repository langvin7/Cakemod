using MegaCrit.Sts2.Core.Saves.Runs; // 确保引入 SavedPropertyAttribute 所在的命名空间
using MegaCrit.Sts2.Core.Models;

namespace cakemod.Scripts
{

    public class CakeSavePropetyModel : AbstractModel
    {
        public override bool ShouldReceiveCombatHooks => false;
        [SavedProperty]
        public int Mod_CurrentDamage { get; set; }

        [SavedProperty]
        public int Mod_CurrentBlock { get; set; }

        [SavedProperty]
        public int Mod_CurrentVulnerable { get; set; }

        [SavedProperty]
        public int Mod_CurrentExtraDamage { get; set; }

        [SavedProperty]
        public int Mod_CurrentIsSelected { get; set; }

        [SavedProperty]
        public int Mod_CurrentHasExhaust { get; set; }

        [SavedProperty]
        public int Mod_CurrentInnate { get; set; }

        [SavedProperty]
        public int Mod_CurrentEthereal { get; set; }

        [SavedProperty]
        public int Mod_CurrentHpLoss { get; set; }

        [SavedProperty]
        public int Mod_CurrentCards { get; set; }

        [SavedProperty]
        public int Mod_CurrentEnergy { get; set; }

        [SavedProperty]
        public int Mod_CurrentStrengthLoss { get; set; }

        [SavedProperty]
        public int Mod_CurrentInfernal { get; set; }

        [SavedProperty]
        public int Mod_CurrentWeak { get; set; }

        [SavedProperty]
        public int Mod_CurrentGrand { get; set; }

        [SavedProperty]
        public int Mod_CurrentSage { get; set; }
    }
}
