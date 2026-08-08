using System.Collections.Generic;
using UnityEngine;

namespace ShellingOut
{
    /// Lives on a scroll view's Content object. shows upgrades that are
    /// unlocked and not yet purchased 
    public class UpgradeListUI : MonoBehaviour
    {
        [SerializeField] UpgradeRowUI rowPrefab;

        readonly List<UpgradeRowUI> rows = new List<UpgradeRowUI>();

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (rowPrefab == null)
            {
                Debug.LogError("[UpgradeListUI] Row Prefab is not assigned. no upgrade rows will be created.", this);
                return;
            }

            foreach (var def in gm.Balance.upgrades)
            {
                if (def == null) continue;
                var row = Instantiate(rowPrefab, transform);
                row.Bind(def);
                rows.Add(row);
            }
            RefreshVisibility();
        }

        void Update() => RefreshVisibility();

        void RefreshVisibility()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            foreach (var row in rows)
            {
                bool visible = !gm.Upgrades.IsPurchased(row.Def) && gm.Upgrades.IsUnlocked(row.Def);
                if (row.gameObject.activeSelf != visible)
                    row.gameObject.SetActive(visible);
            }
        }
    }
}
