using System.Collections.Generic;
using UnityEngine;

namespace ShellingOut
{
    public static class RuntimeDefaults
    {
        public static GameBalance BuildSampleBalance()
        {
            var b = ScriptableObject.CreateInstance<GameBalance>();
            b.name = "GameBalance";
            b.currencyName = "Shells";
            b.premiumCurrencyName = "Pearls";
            b.startingCurrency = 0;
            b.baseClickPower = 1;
            b.autosaveIntervalSeconds = 30f;
            b.offlineCapHours = 8;
            b.offlineEfficiency = 1f;
            b.prestigeBaseRequirement = 1e6;
            b.prestigeExponent = 0.5;
            b.pearlBonusPerUnit = 0.1;

            // Generators climb through the three shell worlds:
            // beach (1-2) -> egg (3-4) -> shotgun (5-6).
            var comber    = Gen("beachcomber", "Beachcomber", "Strolls the shore pocketing washed-up shells.",       15,      1.15, 0.5,   0,       new Color32(0xE8, 0xC9, 0xA0, 0xFF));
            var tideTrap  = Gen("tide_trap",   "Tide Trap",   "Nets a fresh haul every time the tide rolls out.",    100,     1.15, 4,     60,      new Color32(0x9A, 0xC4, 0xE8, 0xFF));
            var henHouse  = Gen("hen_house",   "Hen House",   "The hens are happy to donate their eggshells.",       1100,    1.14, 30,    700,     new Color32(0xF6, 0xEB, 0xD4, 0xFF));
            var hatchery  = Gen("hatchery",    "Hatchery",    "Industrial incubation. The chicks keep the shells coming.", 12000, 1.13, 250, 8000,  new Color32(0xE9, 0xF1, 0xF5, 0xFF));
            var skeet     = Gen("skeet_range", "Shooting Range", "Spent shells rain down all day.",               130000,  1.12, 2000,  90000,   new Color32(0xE8, 0x87, 0x5A, 0xFF));
            var ammoPlant = Gen("ammo_plant",  "Ammo Plant",  "Presses out shells around the clock. Don't ask why.", 1400000, 1.11, 16000, 1000000, new Color32(0xE0, 0x5A, 0x3A, 0xFF));

            b.generators = new List<GeneratorDefinition> { comber, tideTrap, henHouse, hatchery, skeet, ammoPlant };

            var beach   = Shell("beach_shell",   "Beach Shell",   1,  true,  new Color32(0xE8, 0xC9, 0xA0, 0xFF));
            var egg     = Shell("egg_shell",     "Egg Shell",     5,  false, new Color32(0xE9, 0xF1, 0xF5, 0xFF));
            var shotgun = Shell("shotgun_shell", "Shotgun Shell", 25, false, new Color32(0xE0, 0x5A, 0x3A, 0xFF));

            b.shellTypes = new List<ShellTypeDefinition> { beach, egg, shotgun };

            b.upgrades = new List<UpgradeDefinition>
            {
                Upg("sharp_eyes",      "Sharp Eyes",      "Beachcombers spot twice as many shells.",              250,     UpgradeType.GeneratorMultiplier, comber,    2,   100),
                Upg("finer_mesh",      "Finer Mesh",      "A tighter sieve weave: sifting earns twice as much.", 500,     UpgradeType.ClickMultiplier,     null,      2,   200),
                Upg("double_nets",     "Double Nets",     "Tide Traps produce twice as much.",                    1500,    UpgradeType.GeneratorMultiplier, tideTrap,  2,   500),
                UpgShell("egg_hunt",   "Egg Hunt",        "Your sieve turns up Egg Shells: sifts are worth 5x more.", 5000, egg, 2500),
                Upg("shell_market",    "Shell Market",    "Sell smarter: all production increased by 50%.",       10000,   UpgradeType.GlobalMultiplier,    null,      1.5, 5000),
                Upg("free_range",      "Free Range",      "Happier hens: Hen Houses produce twice as much.",      20000,   UpgradeType.GeneratorMultiplier, henHouse,  2,   10000),
                Upg("power_sifter",    "Power Sifter",    "Motorize the sieve: sifting earns three times as much.", 50000, UpgradeType.ClickMultiplier,    null,      3,   25000),
                Upg("twin_incubators", "Twin Incubators", "Hatcheries produce twice as much.",                    150000,  UpgradeType.GeneratorMultiplier, hatchery,  2,   80000),
                UpgShell("locked_and_loaded", "Locked and Loaded", "Your sieve turns up Shotgun Shells: sifts are worth 25x more.", 250000, shotgun, 100000),
                Upg("shell_shock",     "Shell Shock",     "Skeet Ranges produce twice as much.",                  400000,  UpgradeType.GeneratorMultiplier, skeet,     2,   200000),
                Upg("shell_company",   "Shell Company",   "Incorporate. All production doubled.",                 1000000, UpgradeType.GlobalMultiplier,    null,      2,   500000),
                Upg("overtime_shift",  "Overtime Shift",  "Ammo Plants produce twice as much.",                   5000000, UpgradeType.GeneratorMultiplier, ammoPlant, 2,   2500000),
            };

            return b;
        }

        static ShellTypeDefinition Shell(string id, string displayName, double clickValueMultiplier,
            bool unlockedByDefault, Color tint)
        {
            var s = ScriptableObject.CreateInstance<ShellTypeDefinition>();
            s.name = displayName;
            s.id = id;
            s.displayName = displayName;
            s.clickValueMultiplier = clickValueMultiplier;
            s.unlockedByDefault = unlockedByDefault;
            s.tint = tint;
            return s;
        }

        static UpgradeDefinition UpgShell(string id, string displayName, string description,
            double cost, ShellTypeDefinition shellType, double unlockAt)
        {
            var u = Upg(id, displayName, description, cost, UpgradeType.UnlockShellType, null, 1, unlockAt);
            u.targetShellType = shellType;
            return u;
        }

        static GeneratorDefinition Gen(string id, string displayName, string description,
            double baseCost, double costGrowth, double baseProduction, double unlockAt, Color tint)
        {
            var g = ScriptableObject.CreateInstance<GeneratorDefinition>();
            g.name = displayName;
            g.id = id;
            g.displayName = displayName;
            g.description = description;
            g.baseCost = baseCost;
            g.costGrowth = costGrowth;
            g.baseProduction = baseProduction;
            g.unlockAtLifetimeEarnings = unlockAt;
            g.tint = tint;
            return g;
        }

        static UpgradeDefinition Upg(string id, string displayName, string description,
            double cost, UpgradeType type, GeneratorDefinition target, double multiplier, double unlockAt)
        {
            var u = ScriptableObject.CreateInstance<UpgradeDefinition>();
            u.name = displayName;
            u.id = id;
            u.displayName = displayName;
            u.description = description;
            u.cost = cost;
            u.type = type;
            u.targetGenerator = target;
            u.multiplier = multiplier;
            u.unlockAtLifetimeEarnings = unlockAt;
            return u;
        }
    }
}
