using UnityEngine;

namespace ShellingOut
{
    /// One kind of shell the sieve can turn up (beach, egg, shotgun...).
    /// Types are unlocked by UnlockShellType upgrades 
    /// the best unlocked type multiplies click income and is what a sift
    /// visually produces.
    [CreateAssetMenu(fileName = "ShellType", menuName = "Shelling Out/Shell Type")]
    public class ShellTypeDefinition : ScriptableObject
    {
        [Tooltip("Stable id used in content references. Never change after shipping.")]
        public string id;

        public string displayName;

        [Tooltip("Click income multiplier while this is the best unlocked shell.")]
        public double clickValueMultiplier = 1;

        [Tooltip("Available from the start, no unlock upgrade needed.")]
        public bool unlockedByDefault;

        [Header("Presentation")]
        [Tooltip("Art for this shell. Left empty, sift bursts and the pile render it as a plain rectangle colored by the tint below.")]
        public Sprite sprite;
        public Color tint = Color.white;
        [Tooltip("Relative frequency in the shell pile once unlocked; heavier = more common. 0 = never shown in the pile.")]
        public float pileWeight = 1f;
    }
}
