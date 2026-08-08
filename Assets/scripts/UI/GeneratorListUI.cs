using System.Collections.Generic;
using UnityEngine;

namespace ShellingOut
{
    /// Lives on a scroll view's Content object; spawns one GeneratorRowUI per
    /// definition from the Row Prefab and toggles row visibility by unlock
    /// progress.
    public class GeneratorListUI : MonoBehaviour
    {
        [Tooltip("Required: styled row prefab (a GeneratorRowUI with its fields wired). One instance is spawned per generator definition.")]
        [SerializeField] GeneratorRowUI rowPrefab;

        readonly List<GeneratorRowUI> rows = new List<GeneratorRowUI>();

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (rowPrefab == null)
            {
                Debug.LogError("[GeneratorListUI] Row Prefab is not assigned. no generator rows will be created.", this);
                return;
            }

            foreach (var def in gm.Balance.generators)
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
                var state = gm.Generators.Get(row.Def.id);
                bool unlocked = (state != null && state.Owned > 0) ||
                                gm.Currency.LifetimeThisRun >= row.Def.unlockAtLifetimeEarnings;
                if (row.gameObject.activeSelf != unlocked)
                    row.gameObject.SetActive(unlocked);
            }
        }
    }
}
