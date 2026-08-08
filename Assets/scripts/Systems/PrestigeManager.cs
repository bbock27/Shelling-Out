using System;

namespace ShellingOut
{
    /// Prestige loop: reset the run in exchange for pearls with each granting a
    /// permanent production bonus. Points = floor((lifetime / req) ^ exponent).
    public class PrestigeManager
    {
        readonly GameManager gm;

        public double Pearls { get; private set; }

        public PrestigeManager(GameManager gm)
        {
            this.gm = gm;
        }

        public double GlobalMultiplier => 1.0 + Pearls * gm.Balance.pearlBonusPerUnit;

        /// Pearls that would be awarded by prestiging right now.
        public double PendingGain
        {
            get
            {
                var b = gm.Balance;
                double x = gm.Currency.LifetimeThisRun / b.prestigeBaseRequirement;
                if (x <= 0) return 0;
                return Math.Floor(Math.Pow(x, b.prestigeExponent));
            }
        }

        /// Lifetime earnings needed this run to hold `pearlCount` pending pearls.
        public double RequirementFor(double pearlCount) =>
            gm.Balance.prestigeBaseRequirement * Math.Pow(pearlCount, 1.0 / gm.Balance.prestigeExponent);

        public bool CanPrestige => PendingGain >= 1;

        public bool TryPrestige()
        {
            double gain = PendingGain;
            if (gain < 1) return false;

            Pearls += gain;
            gm.Currency.ResetRun(gm.Balance.startingCurrency);
            gm.Upgrades.ResetAll();
            gm.Generators.ResetAll();
            GameEvents.RaisePrestiged(gain);
            gm.SaveNow();
            return true;
        }

        public void Restore(double pearls)
        {
            Pearls = pearls;
        }
    }
}
