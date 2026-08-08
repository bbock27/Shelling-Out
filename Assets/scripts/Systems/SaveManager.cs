using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShellingOut
{
    [Serializable]
    public class SaveData
    {
        public int version = SaveManager.Version;
        public long lastSaveUnix;

        public double currency;
        public double lifetimeThisRun;
        public double totalLifetime;
        public double pearls;

        // Production rate at save time, used to credit offline earnings on load.
        public double productionPerSecond;

        // JsonUtility can't serialize dictionaries so using arrays instead.
        public List<string> generatorIds = new List<string>();
        public List<int> generatorCounts = new List<int>();
        public List<string> upgradeIds = new List<string>();
    }

    /// JSON save/load to persistentDataPath.
    public static class SaveManager
    {
        public const int Version = 1;

        static string FilePath => Path.Combine(Application.persistentDataPath, "shelling_out_save.json");

        public static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static void Save(GameManager gm)
        {
            try
            {
                var data = new SaveData
                {
                    lastSaveUnix = NowUnix(),
                    currency = gm.Currency.Current,
                    lifetimeThisRun = gm.Currency.LifetimeThisRun,
                    totalLifetime = gm.Currency.TotalLifetime,
                    pearls = gm.Prestige.Pearls,
                    productionPerSecond = gm.Generators.TotalProductionPerSecond,
                    upgradeIds = gm.Upgrades.GetPurchasedIds(),
                };
                foreach (var state in gm.Generators.States)
                {
                    data.generatorIds.Add(state.Def.id);
                    data.generatorCounts.Add(state.Owned);
                }
                File.WriteAllText(FilePath, JsonUtility.ToJson(data));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e}");
            }
        }

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed, starting fresh: {e}");
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
    }
}
