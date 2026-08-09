using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShellingOut.EditorTools
{
    public static class IdleGameSetup
    {
        const string DataFolder = "Assets/Data";

        [MenuItem("Tools/Idle Framework/Setup Scene")]
        public static void SetupScene()
        {
            var balance = GetOrCreateBalance();

            var gm = Object.FindAnyObjectByType<GameManager>();
            if (gm == null)
            {
                var go = new GameObject("Game");
                gm = go.AddComponent<GameManager>();
                Undo.RegisterCreatedObjectUndo(go, "Create Game object");
            }

            gm.balance = balance;
            EditorUtility.SetDirty(gm);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("[IdleGameSetup] Data assets + GameManager ready. ");
        }

        [MenuItem("Tools/Idle Framework/Create Sample Data Assets")]
        public static void CreateSampleData()
        {
            GetOrCreateBalance();
            Debug.Log($"[IdleGameSetup] Sample data available in {DataFolder}.");
        }

        [MenuItem("Tools/Idle Framework/Delete Save File")]
        public static void DeleteSave()
        {
            SaveManager.Delete();
            Debug.Log("[IdleGameSetup] Save file deleted.");
        }

        /// Replaces the generator and upgrade assets with the current sample
        /// content. Shell type assets are kept so their art assignments
        /// survive. unlock upgrades are re-pointed at them by id.
        [MenuItem("Tools/Idle Framework/Rebuild Generators + Upgrades")]
        public static void RebuildGeneratorsAndUpgrades()
        {
            var balance = AssetDatabase.LoadAssetAtPath<GameBalance>($"{DataFolder}/GameBalance.asset");
            if (balance == null)
            {
                GetOrCreateBalance();
                Debug.Log("[IdleGameSetup] No existing GameBalance found. Created fresh sample data instead.");
                return;
            }

            EnsureShellContent(balance);

            DeleteFolderAssets($"{DataFolder}/Generators");
            DeleteFolderAssets($"{DataFolder}/Upgrades");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/Generators"))
                AssetDatabase.CreateFolder(DataFolder, "Generators");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/Upgrades"))
                AssetDatabase.CreateFolder(DataFolder, "Upgrades");

            var sample = RuntimeDefaults.BuildSampleBalance();

            // Point the upgrades' shell references (unlock targets and gates)
            // at the shell type assets already on disk.
            foreach (var upg in sample.upgrades)
            {
                if (upg.targetShellType != null)
                {
                    var onDisk = balance.shellTypes.Find(s => s != null && s.id == upg.targetShellType.id);
                    if (onDisk != null) upg.targetShellType = onDisk;
                }
                if (upg.requiredShellType != null)
                {
                    var onDisk = balance.shellTypes.Find(s => s != null && s.id == upg.requiredShellType.id);
                    if (onDisk != null) upg.requiredShellType = onDisk;
                }
            }

            // Same for the generators' shell gates.
            foreach (var gen in sample.generators)
            {
                if (gen.requiredShellType == null) continue;
                var onDisk = balance.shellTypes.Find(s => s != null && s.id == gen.requiredShellType.id);
                if (onDisk != null) gen.requiredShellType = onDisk;
            }

            foreach (var gen in sample.generators)
                AssetDatabase.CreateAsset(gen, $"{DataFolder}/Generators/{gen.name}.asset");
            foreach (var upg in sample.upgrades)
                AssetDatabase.CreateAsset(upg, $"{DataFolder}/Upgrades/{upg.name}.asset");

            balance.generators = new System.Collections.Generic.List<GeneratorDefinition>(sample.generators);
            balance.upgrades = new System.Collections.Generic.List<UpgradeDefinition>(sample.upgrades);
            EditorUtility.SetDirty(balance);
            AssetDatabase.SaveAssets();

            Debug.Log("[IdleGameSetup] Generators + upgrades rebuilt from sample content. " +
                      "Old saves keep currency/pearls, but counts for removed generator ids are discarded " +
                      "consider Tools > Idle Framework > Delete Save File for a clean run.");
        }

        static void DeleteFolderAssets(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;
            foreach (var guid in AssetDatabase.FindAssets("", new[] { folder }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        static GameBalance GetOrCreateBalance()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameBalance>($"{DataFolder}/GameBalance.asset");
            if (existing != null)
            {
                EnsureShellContent(existing);
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/Generators"))
                AssetDatabase.CreateFolder(DataFolder, "Generators");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/ShellTypes"))
                AssetDatabase.CreateFolder(DataFolder, "ShellTypes");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/Upgrades"))
                AssetDatabase.CreateFolder(DataFolder, "Upgrades");

            // Build the in-memory sample content, then persist every piece as
            // an asset (shell types first: generators and upgrades reference them).
            var balance = RuntimeDefaults.BuildSampleBalance();
            foreach (var shell in balance.shellTypes)
                AssetDatabase.CreateAsset(shell, $"{DataFolder}/ShellTypes/{shell.name}.asset");
            foreach (var gen in balance.generators)
                AssetDatabase.CreateAsset(gen, $"{DataFolder}/Generators/{gen.name}.asset");
            foreach (var upg in balance.upgrades)
                AssetDatabase.CreateAsset(upg, $"{DataFolder}/Upgrades/{upg.name}.asset");
            AssetDatabase.CreateAsset(balance, $"{DataFolder}/GameBalance.asset");
            AssetDatabase.SaveAssets();

            return balance;
        }

        /// Upgrades a GameBalance asset created before shell types existed:
        /// adds the sample shell type assets and their unlock upgrades.
        static void EnsureShellContent(GameBalance balance)
        {
            if (balance.shellTypes != null && balance.shellTypes.Count > 0) return;

            if (!AssetDatabase.IsValidFolder($"{DataFolder}/ShellTypes"))
                AssetDatabase.CreateFolder(DataFolder, "ShellTypes");
            if (!AssetDatabase.IsValidFolder($"{DataFolder}/Upgrades"))
                AssetDatabase.CreateFolder(DataFolder, "Upgrades");

            var sample = RuntimeDefaults.BuildSampleBalance();
            foreach (var shell in sample.shellTypes)
                AssetDatabase.CreateAsset(shell, $"{DataFolder}/ShellTypes/{shell.name}.asset");
            balance.shellTypes = new System.Collections.Generic.List<ShellTypeDefinition>(sample.shellTypes);

            foreach (var upg in sample.upgrades)
            {
                if (upg.type != UpgradeType.UnlockShellType) continue;
                if (balance.upgrades.Exists(u => u != null && u.id == upg.id)) continue;
                AssetDatabase.CreateAsset(upg, $"{DataFolder}/Upgrades/{upg.name}.asset");
                balance.upgrades.Add(upg);
            }

            EditorUtility.SetDirty(balance);
            AssetDatabase.SaveAssets();
            Debug.Log("[IdleGameSetup] Added shell types + unlock upgrades to the existing GameBalance.");
        }
    }
}
