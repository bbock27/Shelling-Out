using System;

namespace ShellingOut
{
    /// Global game events. UI mostly polls for display, but these hooks let
    /// popups, audio, particles, analytics, etc. react without coupling to systems.
    public static class GameEvents
    {
        public static event Action<double> CurrencyChanged;
        public static event Action<double> Clicked;                      // click power gained
        public static event Action<GeneratorState> GeneratorPurchased;
        public static event Action<UpgradeDefinition> UpgradePurchased;
        public static event Action<double> Prestiged;                    // pearls gained
        public static event Action<double, double> OfflineEarnings;      // amount, seconds away

        public static void RaiseCurrencyChanged(double current) => CurrencyChanged?.Invoke(current);
        public static void RaiseClicked(double amount) => Clicked?.Invoke(amount);
        public static void RaiseGeneratorPurchased(GeneratorState state) => GeneratorPurchased?.Invoke(state);
        public static void RaiseUpgradePurchased(UpgradeDefinition def) => UpgradePurchased?.Invoke(def);
        public static void RaisePrestiged(double gained) => Prestiged?.Invoke(gained);
        public static void RaiseOfflineEarnings(double amount, double seconds) => OfflineEarnings?.Invoke(amount, seconds);

        public static void Clear()
        {
            CurrencyChanged = null;
            Clicked = null;
            GeneratorPurchased = null;
            UpgradePurchased = null;
            Prestiged = null;
            OfflineEarnings = null;
        }
    }
}
