using System.Collections.Generic;
using UnityEngine;

namespace ShellingOut
{
    /// Top-level tuning asset: global economy numbers plus the full content
    /// lists. The GameManager needs exactly one of these.
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Shelling Out/Game Balance")]
    public class GameBalance : ScriptableObject
    {
        [Header("Naming")]
        public string currencyName = "Shells";
        public string premiumCurrencyName = "Pearls";

        [Header("Start & Clicking")]
        public double startingCurrency = 0;
        public double baseClickPower = 1;

        [Header("Saving")]
        public float autosaveIntervalSeconds = 30f;

        [Header("Offline Progress")]
        [Tooltip("Maximum hours of offline production credited on return.")]
        public double offlineCapHours = 8;
        [Range(0f, 1f)]
        [Tooltip("Fraction of normal production earned while away.")]
        public float offlineEfficiency = 1f;

        [Header("Prestige")]
        [Tooltip("Lifetime earnings that award the first prestige point.")]
        public double prestigeBaseRequirement = 1e6;
        [Tooltip("Points = floor((lifetime / requirement) ^ exponent).")]
        public double prestigeExponent = 0.5;
        [Tooltip("Production bonus per prestige point (0.1 = +10% each).")]
        public double pearlBonusPerUnit = 0.1;

        [Header("Presentation")]
        [Tooltip("Art for the sieve button, applied to its Image at Play. Left empty, the sieve keeps whatever the scene authored (no sprite = plain tinted rect).")]
        public Sprite sieveSprite;
        [Tooltip("Fallback art for the shell pile when ShellPileUI's own sprite list is empty. Left empty too, pile shells render as plain tinted rectangles.")]
        public Sprite shellSprite;

        [Header("Content")]
        public List<GeneratorDefinition> generators = new List<GeneratorDefinition>();
        public List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
        [Tooltip("Shell kinds the sieve can produce, in ascending value order.")]
        public List<ShellTypeDefinition> shellTypes = new List<ShellTypeDefinition>();
    }
}
