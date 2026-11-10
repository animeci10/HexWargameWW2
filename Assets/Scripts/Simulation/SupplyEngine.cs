using System.Collections.Generic;
using HexWargame.Core;
using HexWargame.Data;

namespace HexWargame.Simulation
{
    public class SupplyEngine
    {
        public delegate bool IsHexBlockingSupplyDelegate(HexCoordinates coords, int teamId);

        private readonly IsHexBlockingSupplyDelegate isBlockingSupply;

        public SupplyEngine(IsHexBlockingSupplyDelegate blockingChecker)
        {
            this.isBlockingSupply = blockingChecker;
        }

        /// <summary>
        /// Haritadaki tüm birliklerin ikmal (Supply) durumunu günceller.
        /// </summary>
        public void EvaluateSupplyNetwork(
            List<UnitInstance> allUnits, 
            List<HexCoordinates> supplyDepots, 
            int maxSupplyRadius = 12)
        {
            Dictionary<int, HashSet<HexCoordinates>> teamSuppliedHexes = new();

            // Her takım için ikmal depolarından yayılan ikmal ağını hesapla (Dijkstra/BFS)
            foreach (var depot in supplyDepots)
            {
                // Deponun hangi takıma ait olduğu belirlenir
                int depotTeam = 0; // Varsayılan takım kontrolü
                if (!teamSuppliedHexes.ContainsKey(depotTeam))
                {
                    teamSuppliedHexes[depotTeam] = new HashSet<HexCoordinates>();
                }

                CalculateSupplyRadius(depot, depotTeam, maxSupplyRadius, teamSuppliedHexes[depotTeam]);
            }

            // Birlikleri kontrol et
            foreach (var unit in allUnits)
            {
                bool isSupplied = false;
                if (teamSuppliedHexes.TryGetValue(unit.teamId, out var suppliedHexes))
                {
                    isSupplied = suppliedHexes.Contains(unit.coordinates);
                }

                unit.isOutOfSupply = !isSupplied;

                if (isSupplied)
                {
                    // İkmal sağlamsa mühimmat ve yakıt yenilenir
                    unit.currentAmmo = unit.blueprint.ammoCapacity;
                    unit.currentFuel = unit.blueprint.fuelCapacity;
                }
            }
        }

        private void CalculateSupplyRadius(
            HexCoordinates depot, 
            int teamId, 
            int maxRadius, 
            HashSet<HexCoordinates> outSuppliedHexes)
        {
            Queue<(HexCoordinates coords, int dist)> queue = new();
            HashSet<HexCoordinates> visited = new();

            queue.Enqueue((depot, 0));
            visited.Add(depot);
            outSuppliedHexes.Add(depot);

            while (queue.Count > 0)
            {
                var (curr, dist) = queue.Dequeue();

                if (dist >= maxRadius) continue;

                foreach (var neighbor in curr.GetAllNeighbors())
                {
                    if (visited.Contains(neighbor)) continue;

                    // Düşman birlikleri veya düşman ZoC'u ikmali engeller
                    if (isBlockingSupply != null && isBlockingSupply(neighbor, teamId))
                        continue;

                    visited.Add(neighbor);
                    outSuppliedHexes.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
    }
}
