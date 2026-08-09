using System.Collections.Generic;

namespace ShellingOut
{
    /// Tracks purchased one time upgrades and caches their combined multipliers.
    public class UpgradeManager
    {
        readonly GameManager gm;
        readonly HashSet<string> purchased = new HashSet<string>();
        readonly Dictionary<string, double> generatorMultipliers = new Dictionary<string, double>();
        readonly HashSet<string> unlockedShellIds = new HashSet<string>();

        public double GlobalMultiplier { get; private set; } = 1;
        public double ClickMultiplier { get; private set; } = 1;

        public UpgradeManager(GameManager gm)
        {
            this.gm = gm;
            // Register default-unlocked shell types immediately -- on a fresh
            // start (no save) nothing else triggers a Recalculate.
            Recalculate();
        }

        public bool IsPurchased(UpgradeDefinition def) => def != null && purchased.Contains(def.id);

        public bool IsUnlocked(UpgradeDefinition def) =>
            gm.Currency.LifetimeThisRun >= def.unlockAtLifetimeEarnings &&
            (def.requiredShellType == null || IsShellUnlocked(def.requiredShellType));

        public double GetGeneratorMultiplier(string generatorId) =>
            generatorMultipliers.TryGetValue(generatorId, out var m) ? m : 1.0;

        public bool IsShellUnlocked(ShellTypeDefinition shell) =>
            shell != null && unlockedShellIds.Contains(shell.id);

        public bool TryBuy(UpgradeDefinition def)
        {
            if (def == null || IsPurchased(def)) return false;
            // Gated upgrades can't be bought until their shell type is unlocked.
            if (def.requiredShellType != null && !IsShellUnlocked(def.requiredShellType)) return false;
            if (!gm.Currency.Spend(def.cost)) return false;
            purchased.Add(def.id);
            Recalculate();
            GameEvents.RaiseUpgradePurchased(def);
            return true;
        }

        public void Recalculate()
        {
            generatorMultipliers.Clear();
            GlobalMultiplier = 1;
            ClickMultiplier = 1;

            unlockedShellIds.Clear();
            foreach (var shell in gm.Balance.shellTypes)
                if (shell != null && shell.unlockedByDefault)
                    unlockedShellIds.Add(shell.id);

            foreach (var def in gm.Balance.upgrades)
            {
                if (def == null || !purchased.Contains(def.id)) continue;
                switch (def.type)
                {
                    case UpgradeType.GeneratorMultiplier:
                        if (def.targetGenerator != null)
                        {
                            string id = def.targetGenerator.id;
                            generatorMultipliers[id] = GetGeneratorMultiplier(id) * def.multiplier;
                        }
                        break;
                    case UpgradeType.GlobalMultiplier:
                        GlobalMultiplier *= def.multiplier;
                        break;
                    case UpgradeType.ClickMultiplier:
                        ClickMultiplier *= def.multiplier;
                        break;
                    case UpgradeType.UnlockShellType:
                        if (def.targetShellType != null)
                            unlockedShellIds.Add(def.targetShellType.id);
                        break;
                }
            }
        }

        public void ResetAll()
        {
            purchased.Clear();
            Recalculate();
        }

        public void Restore(IEnumerable<string> ids)
        {
            purchased.Clear();
            if (ids != null)
                foreach (var id in ids)
                    purchased.Add(id);
            Recalculate();
        }

        public List<string> GetPurchasedIds() => new List<string>(purchased);
    }
}
