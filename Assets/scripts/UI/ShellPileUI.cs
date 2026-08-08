using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// A mound of small shells at the bottom of the play area that grows
    /// with current currency on a log scale. 
    /// Placement is deterministic per index.
    public class ShellPileUI : MonoBehaviour
    {
        const int MaxShells = 120;
        const float ShellWidth = 92f;
        const float ShellHeight = 74f;
        const float SpacingX = 58f;
        const float RowHeight = 40f;

        [Tooltip("Sprite variants scattered through the pile; each shell picks one deterministically. Left empty, falls back to GameBalance.shellSprite, then plain tinted rectangles.")]
        public List<Sprite> shellSprites = new List<Sprite>();

        readonly List<Image> shells = new List<Image>();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

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

        /// ~10 shells per order of magnitude: 1K -> 30, 1M -> 60, 1B -> 90.
        static int TargetCount(double currency) =>
            (int)Math.Min(MaxShells, Math.Floor(10.0 * Math.Log10(1.0 + currency)));

        void AddShell(int index)
        {
            var go = new GameObject($"Shell_{index}", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = PickSprite(index);
            img.preserveAspect = true;
            img.raycastTarget = false;

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

            // Subtle per-shell tint variation.
            float value = 0.88f + 0.14f * u;
            img.color = new Color(value, value * 0.97f, value * 0.93f, 1f);

            shells.Add(img); // creation order = draw order, so higher rows render on top
        }

        /// Deterministic pick from the variant list (stable per index, like
        /// placement); falls back to GameBalance.shellSprite, then to no
        /// sprite at all (a plain tinted rectangle placeholder).
        Sprite PickSprite(int index)
        {
            if (shellSprites != null && shellSprites.Count > 0)
            {
                var sprite = shellSprites[(int)(Hash(index * 2 + 3) * shellSprites.Count)];
                if (sprite != null) return sprite;
            }
            var gm = GameManager.Instance;
            if (gm != null && gm.Balance != null && gm.Balance.shellSprite != null)
                return gm.Balance.shellSprite;
            return null;
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
