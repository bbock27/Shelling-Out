using System;
using UnityEngine;

namespace ShellingOut
{
    /// Central hub that owns the systems, drives the production tick, autosaves,
    /// and applies offline earnings on load
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Tuning + content. Left empty, built-in sample content is used.")]
        public GameBalance balance;

        public GameBalance Balance => balance;
        public CurrencyManager Currency { get; private set; }
        public GeneratorManager Generators { get; private set; }
        public UpgradeManager Upgrades { get; private set; }
        public PrestigeManager Prestige { get; private set; }

        /// Multiplier applied to everything (upgrades x prestige).
        public double GlobalMultiplier => Upgrades.GlobalMultiplier * Prestige.GlobalMultiplier;

        /// what a sift produces.
        public ShellTypeDefinition CurrentShellType
        {
            get
            {
                ShellTypeDefinition best = null;
                foreach (var shell in balance.shellTypes)
                {
                    if (shell == null || !Upgrades.IsShellUnlocked(shell)) continue;
                    if (best == null || shell.clickValueMultiplier > best.clickValueMultiplier)
                        best = shell;
                }
                return best;
            }
        }

        public double ClickPower =>
            balance.baseClickPower * Upgrades.ClickMultiplier * GlobalMultiplier *
            (CurrentShellType != null ? CurrentShellType.clickValueMultiplier : 1.0);

        bool initialized;
        float autosaveTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Instance = null;
            GameEvents.Clear();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (balance == null)
            {
                Debug.Log("[GameManager] No GameBalance assigned -- using built-in sample content.");
                balance = RuntimeDefaults.BuildSampleBalance();
            }

            Currency = new CurrencyManager();
            Upgrades = new UpgradeManager(this);
            Prestige = new PrestigeManager(this);
            Generators = new GeneratorManager(this);
        }

        void Start()
        {
            var data = SaveManager.Load();
            if (data != null)
            {
                Currency.Restore(data.currency, data.lifetimeThisRun, data.totalLifetime);
                Prestige.Restore(data.pearls);
                Upgrades.Restore(data.upgradeIds);
                Generators.Restore(data.generatorIds, data.generatorCounts);
                ApplyOfflineProgress(data);
            }
            else if (balance.startingCurrency > 0)
            {
                Currency.Add(balance.startingCurrency);
            }
            initialized = true;
        }

        void ApplyOfflineProgress(SaveData data)
        {
            double secondsAway = Math.Max(0, SaveManager.NowUnix() - data.lastSaveUnix);
            double credited = Math.Min(secondsAway, balance.offlineCapHours * 3600.0);
            double gain = data.productionPerSecond * credited * balance.offlineEfficiency;

            // Ignore trivial absences so the popup isn't noise on every domain reload.
            if (gain <= 0 || secondsAway < 60) return;

            Currency.Add(gain);
            GameEvents.RaiseOfflineEarnings(gain, credited);
        }

        void Update()
        {
            if (!initialized) return;

            Generators.Tick(Time.deltaTime);

            autosaveTimer += Time.deltaTime;
            if (autosaveTimer >= balance.autosaveIntervalSeconds)
            {
                autosaveTimer = 0f;
                SaveNow();
            }
        }

        /// Manual click / tap on the collect button.
        public double Click()
        {
            double amount = ClickPower;
            Currency.Add(amount);
            GameEvents.RaiseClicked(amount);
            return amount;
        }

        public void SaveNow()
        {
            if (!initialized) return;
            SaveManager.Save(this);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        void OnApplicationQuit()
        {
            SaveNow();
        }

        [ContextMenu("Wipe Save File")]
        void WipeSave()
        {
            SaveManager.Delete();
            Debug.Log("[GameManager] Save file deleted. Restart play mode for a fresh run.");
        }
    }
}
