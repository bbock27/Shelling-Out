using UnityEngine;

namespace ShellingOut
{
    /// Authoring data for one production building ("generator").
    /// Cost scales geometrically: cost(n) = baseCost * costGrowth^owned.
    [CreateAssetMenu(fileName = "Generator", menuName = "Shelling Out/Generator")]
    public class GeneratorDefinition : ScriptableObject
    {
        [Tooltip("Stable id used in save files. Never change after shipping.")]
        public string id;

        public string displayName;
        [TextArea] public string description;

        [Header("Economy")]
        public double baseCost = 15;
        public double costGrowth = 1.15;
        [Tooltip("Currency produced per second by ONE unit, before multipliers.")]
        public double baseProduction = 1;

        [Header("Progression")]
        [Tooltip("Row appears once lifetime earnings this run reach this value.")]
        public double unlockAtLifetimeEarnings = 0;
        [Tooltip("Shell type that must be unlocked before this generator can appear or be bought. Empty = no gate.")]
        public ShellTypeDefinition requiredShellType;

        [Header("Presentation")]
        public Sprite icon;
        public Color tint = Color.white;
    }
}
