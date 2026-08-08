using System;
using System.Collections.Generic;

namespace ShellingOut
{
    public enum BuyAmount { One, Ten, Max }

    /// Runtime state for one generator.
    public class GeneratorState
    {
        public GeneratorDefinition Def;
        public int Owned;
    }

    /// Owns generator counts, production ticking, and purchase math
    public class GeneratorManager
    {
        const int MaxBulkBuy = 100000;

        readonly GameManager gm;
        readonly List<GeneratorState> states = new List<GeneratorState>();
        readonly Dictionary<string, GeneratorState> byId = new Dictionary<string, GeneratorState>();

        public BuyAmount CurrentBuyAmount = BuyAmount.One;
        public IReadOnlyList<GeneratorState> States => states;

        public GeneratorManager(GameManager gm)
        {
            this.gm = gm;
            foreach (var def in gm.Balance.generators)
            {
                if (def == null) continue;
                var state = new GeneratorState { Def = def };
                states.Add(state);
                byId[def.id] = state;
            }
        }

        public GeneratorState Get(string id) => byId.TryGetValue(id, out var s) ? s : null;

        /// Production per second of a single unit, all multipliers applied.
        public double UnitProduction(GeneratorState s) =>
            s.Def.baseProduction * gm.Upgrades.GetGeneratorMultiplier(s.Def.id) * gm.GlobalMultiplier;

        public double ProductionOf(GeneratorState s) => UnitProduction(s) * s.Owned;

        public double TotalProductionPerSecond
        {
            get
            {
                double total = 0;
                foreach (var s in states) total += ProductionOf(s);
                return total;
            }
        }

        public void Tick(float deltaTime)
        {
            double total = TotalProductionPerSecond;
            if (total > 0) gm.Currency.Add(total * deltaTime);
        }

        /// Cost of buying 'count' units starting from the current owned amount.
        public double CostOf(GeneratorState s, int count)
        {
            if (count <= 0) return 0;
            double g = s.Def.costGrowth;
            double firstCost = s.Def.baseCost * Math.Pow(g, s.Owned);
            if (Math.Abs(g - 1.0) < 1e-9) return firstCost * count;
            return firstCost * (Math.Pow(g, count) - 1) / (g - 1);
        }

        /// Largest purchase the player can currently afford (0 if none).
        public int MaxAffordable(GeneratorState s)
        {
            double funds = gm.Currency.Current;
            double g = s.Def.costGrowth;
            double firstCost = s.Def.baseCost * Math.Pow(g, s.Owned);
            if (funds < firstCost) return 0;

            int k;
            if (Math.Abs(g - 1.0) < 1e-9)
            {
                k = (int)Math.Floor(funds / firstCost);
            }
            else
            {
                k = (int)Math.Floor(Math.Log(funds * (g - 1) / firstCost + 1, g));
                // Guard against floating point drift on the boundary.
                while (k > 0 && CostOf(s, k) > funds) k--;
                while (k < MaxBulkBuy && CostOf(s, k + 1) <= funds) k++;
            }
            return Math.Min(k, MaxBulkBuy);
        }

        /// How many units the current buy mode would purchase right now.
        public int ResolveBuyCount(GeneratorState s)
        {
            switch (CurrentBuyAmount)
            {
                case BuyAmount.Ten: return 10;
                case BuyAmount.Max: return Math.Max(1, MaxAffordable(s));
                default: return 1;
            }
        }

        public bool TryBuy(GeneratorState s)
        {
            int count = ResolveBuyCount(s);
            double cost = CostOf(s, count);
            if (!gm.Currency.Spend(cost)) return false;
            s.Owned += count;
            GameEvents.RaiseGeneratorPurchased(s);
            return true;
        }

        public void ResetAll()
        {
            foreach (var s in states) s.Owned = 0;
        }

        public void Restore(List<string> ids, List<int> counts)
        {
            if (ids == null || counts == null) return;
            for (int i = 0; i < ids.Count && i < counts.Count; i++)
            {
                var state = Get(ids[i]);
                if (state != null) state.Owned = counts[i];
            }
        }
    }
}
