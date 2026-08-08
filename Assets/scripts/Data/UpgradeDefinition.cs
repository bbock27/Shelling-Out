using UnityEngine;

namespace ShellingOut
{
    public enum UpgradeType
    {
        /// Multiplies one generator's production.
        GeneratorMultiplier,
        /// Multiplies all production and click power.
        GlobalMultiplier,
        /// Multiplies click power only.
        ClickMultiplier,
        /// Unlocks a shell type (see targetShellType).
        UnlockShellType,
    }

    /// One-time purchasable upgrade.
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Shelling Out/Upgrade")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Tooltip("Stable id used in save files. Never change after shipping.")]
        public string id;

        public string displayName;
        [TextArea] public string description;

        [Header("Economy")]
        public double cost = 100;

        [Header("Effect")]
        public UpgradeType type = UpgradeType.GeneratorMultiplier;
        [Tooltip("Only used when type is GeneratorMultiplier.")]
        public GeneratorDefinition targetGenerator;
        [Tooltip("Only used when type is UnlockShellType.")]
        public ShellTypeDefinition targetShellType;
        [Tooltip("Not used by UnlockShellType (the shell's own value applies).")]
        public double multiplier = 2;

        [Header("Progression")]
        [Tooltip("Row appears once lifetime earnings this run reach this value.")]
        public double unlockAtLifetimeEarnings = 0;
    }
}
