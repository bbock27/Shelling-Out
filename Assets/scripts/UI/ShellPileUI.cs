using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// A mound of small shells at the bottom of the play area that grows
    /// with current currency on a log scale.
    /// Placement is deterministic per index.
    /// The mix reflects the unlocked shell types: each shell picks a type by
    /// pileWeight (heavier = more common), so better shells read as rarer.
    /// When the unlocked set changes (unlock bought, prestige) the pile
    /// restyles in place.
    public class ShellPileUI : MonoBehaviour
    {
        const int MaxShells = 120;
        const float ShellWidth = 92f;
        const float ShellHeight = 74f;
        const float SpacingX = 58f;
        const float RowHeight = 40f;

        readonly List<Image> shells = new List<Image>();
        readonly List<ShellTypeDefinition> unlockedTypes = new List<ShellTypeDefinition>();
        float totalWeight;
        int unlockedSignature = -1;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            RefreshUnlockedTypes(gm);

            int target = TargetCount(gm.Currency.Current);
            while (shells.Count < target)
                AddShell(shells.Count);
            while (shells.Count > target)
            {
                var last = shells[shells.Count - 1];
                shells.RemoveAt(shells.Count - 1);
                if (last != null) Destroy(last.gameObject);
            }
        }

        /// Rebuilds the unlocked-type cache when the set changes and restyles
        /// the existing shells to match the new mix.
        void RefreshUnlockedTypes(GameManager gm)
        {
            var types = gm.Balance.shellTypes;
            int signature = 0;
            for (int i = 0; i < types.Count; i++)
                if (types[i] != null && gm.Upgrades.IsShellUnlocked(types[i]))
                    signature |= 1 << (i % 31);
            if (signature == unlockedSignature) return;
            unlockedSignature = signature;

            unlockedTypes.Clear();
            totalWeight = 0f;
            foreach (var type in types)
            {
                if (type == null || !gm.Upgrades.IsShellUnlocked(type)) continue;
                unlockedTypes.Add(type);
                totalWeight += Mathf.Max(0f, type.pileWeight);
            }

            for (int i = 0; i < shells.Count; i++)
                if (shells[i] != null) Style(shells[i], i);
        }

        /// ~10 shells per order of magnitude: 1K -> 30, 1M -> 60, 1B -> 90.
        static int TargetCount(double currency) =>
            (int)Math.Min(MaxShells, Math.Floor(10.0 * Math.Log10(1.0 + currency)));

        void AddShell(int index)
        {
            var go = new GameObject($"Shell_{index}", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;
            Style(img, index);

            float u = Hash(index * 2 + 1);
            float w = Hash(index * 2 + 2);
            var (row, col, capacity) = Place(index);

            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); // bottom center of the pile area
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(ShellWidth, ShellHeight);
            rt.anchoredPosition = new Vector2(
                (col - (capacity - 1) * 0.5f) * SpacingX + (u - 0.5f) * 26f,
                36f + row * RowHeight + (w - 0.5f) * 14f);
            rt.localRotation = Quaternion.Euler(0f, 0f, (u - 0.5f) * 40f);
            rt.localScale = Vector3.one * (0.85f + 0.3f * w);

            shells.Add(img); // creation order = draw order, so higher rows render on top
        }

        /// Applies a shell's type sprite and tint (deterministic per index),
        /// combined with a subtle per-shell brightness variation.
        void Style(Image img, int index)
        {
            var type = PickType(index);
            img.sprite = type != null ? type.sprite : null; // null = plain tinted rect
            var tint = type != null ? type.tint : Color.white;

            float u = Hash(index * 2 + 1);
            float value = 0.88f + 0.14f * u;
            img.color = new Color(tint.r * value, tint.g * value * 0.97f, tint.b * value * 0.93f, 1f);
        }

        /// Weighted pick from the unlocked types (heavier pileWeight = more
        /// common), stable per index like placement.
        ShellTypeDefinition PickType(int index)
        {
            if (unlockedTypes.Count == 0) return null;
            float roll = Hash(index * 2 + 3);

            if (totalWeight <= 0f) // all weights zero: uniform
                return unlockedTypes[(int)(roll * unlockedTypes.Count)];

            float cursor = roll * totalWeight;
            foreach (var type in unlockedTypes)
            {
                cursor -= Mathf.Max(0f, type.pileWeight);
                if (cursor < 0f) return type;
            }
            return unlockedTypes[unlockedTypes.Count - 1];
        }

        /// Pyramid packing: wide bottom rows, narrowing upward.
        static (int row, int col, int capacity) Place(int index)
        {
            int row = 0, capacity = 24;
            while (index >= capacity)
            {
                index -= capacity;
                row++;
                capacity = Math.Max(6, 24 - 3 * row);
            }
            return (row, index, capacity);
        }

        /// Deterministic pseudo-random in [0,1) from an int (PCG-style).
        static float Hash(int n)
        {
            unchecked
            {
                uint x = (uint)n * 747796405u + 2891336453u;
                x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
                x ^= x >> 22;
                return (x & 0xFFFFFF) / 16777216f;
            }
        }
    }
}
