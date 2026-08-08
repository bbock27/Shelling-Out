using System;

namespace ShellingOut
{
    /// Holds the main currency. LifetimeThisRun drives unlocks and the
    /// prestige formula. TotalLifetime persists across prestiges for stats.
    public class CurrencyManager
    {
        const double Epsilon = 1e-9;

        public double Current { get; private set; }
        public double LifetimeThisRun { get; private set; }
        public double TotalLifetime { get; private set; }

        public bool CanAfford(double amount) => Current >= amount - Epsilon;

        public void Add(double amount)
        {
            if (amount <= 0) return;
            Current += amount;
            LifetimeThisRun += amount;
            TotalLifetime += amount;
            GameEvents.RaiseCurrencyChanged(Current);
        }

        public bool Spend(double amount)
        {
            if (amount < 0 || !CanAfford(amount)) return false;
            Current = Math.Max(0, Current - amount);
            GameEvents.RaiseCurrencyChanged(Current);
            return true;
        }

        /// Prestige reset: wipes the run but keeps all-time stats.
        public void ResetRun(double startingCurrency)
        {
            Current = startingCurrency;
            LifetimeThisRun = 0;
            GameEvents.RaiseCurrencyChanged(Current);
        }

        public void Restore(double current, double lifetimeThisRun, double totalLifetime)
        {
            Current = current;
            LifetimeThisRun = lifetimeThisRun;
            TotalLifetime = totalLifetime;
            GameEvents.RaiseCurrencyChanged(Current);
        }
    }
}
