using System.Collections.Generic;
using HexWargame.Core;
using HexWargame.Data;
using UnityEngine;

namespace HexWargame.Simulation
{
    public class HexPathfinder
    {
        public delegate TerrainDataSO GetTerrainDelegate(HexCoordinates coords);
        public delegate UnitInstance GetUnitDelegate(HexCoordinates coords);

        private readonly GetTerrainDelegate getTerrain;
        private readonly GetUnitDelegate getUnit;

        public HexPathfinder(GetTerrainDelegate terrainGetter, GetUnitDelegate unitGetter)
        {
            this.getTerrain = terrainGetter;
            this.getUnit = unitGetter;
        }

        /// <summary>
        /// Birlik için hareket edilebilir tüm hücreleri (Movement Range) hesaplar.
        /// </summary>
        public HashSet<HexCoordinates> GetReachableCoordinates(UnitInstance unit)
        {
            HashSet<HexCoordinates> reachable = new HashSet<HexCoordinates>();
            Dictionary<HexCoordinates, int> costSoFar = new Dictionary<HexCoordinates, int>();

            var frontier = new PriorityQueue<HexCoordinates, int>();
            frontier.Enqueue(unit.coordinates, 0);
            costSoFar[unit.coordinates] = 0;

            int maxMP = unit.currentMP;

            while (frontier.Count > 0)
            {
                HexCoordinates current = frontier.Dequeue();

                foreach (HexCoordinates next in current.GetAllNeighbors())
                {
                    TerrainDataSO terrain = getTerrain(next);
                    if (terrain == null) continue;

                    int stepCost = terrain.GetMovementCost(unit.blueprint.movementType);
                    if (stepCost >= TerrainDataSO.IMPASSIBLE) continue;

                    // Düşman birimi varsa doğrudan üstüne yürünemez (Saldırı menzili ayrıdır)
                    UnitInstance occupant = getUnit(next);
                    if (occupant != null && occupant.teamId != unit.teamId) continue;

                    // Zone of Control (ZoC) Kontrolü
                    bool enteredZoC = !unit.blueprint.ignoresZoneOfControl && IsInEnemyZoneOfControl(next, unit.teamId);
                    int totalNewCost = costSoFar[current] + stepCost;

                    if (enteredZoC)
                    {
                        // ZoC'a girildiğinde birimin kalan tüm hareket puanı tükenir
                        totalNewCost = Mathf.Max(totalNewCost, maxMP);
                    }

                    if (totalNewCost <= maxMP)
                    {
                        if (!costSoFar.ContainsKey(next) || totalNewCost < costSoFar[next])
                        {
                            costSoFar[next] = totalNewCost;
                            frontier.Enqueue(next, totalNewCost);
                            
                            // Üzerinde dost birlik varsa geçilebilir ama orada durulamaz
                            if (occupant == null)
                            {
                                reachable.Add(next);
                            }
                        }
                    }
                }
            }

            return reachable;
        }

        /// <summary>
        /// Hedef noktada düşman Zone of Control'ü var mı?
        /// </summary>
        public bool IsInEnemyZoneOfControl(HexCoordinates coords, int friendlyTeamId)
        {
            foreach (HexCoordinates neighbor in coords.GetAllNeighbors())
            {
                UnitInstance unit = getUnit(neighbor);
                if (unit != null && unit.teamId != friendlyTeamId && !unit.isSuppressed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A* algoritması ile en kısa ve en az maliyetli yolu bulur.
        /// </summary>
        public List<HexCoordinates> FindPath(UnitInstance unit, HexCoordinates target)
        {
            Dictionary<HexCoordinates, HexCoordinates> cameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
            Dictionary<HexCoordinates, int> costSoFar = new Dictionary<HexCoordinates, int>();

            var frontier = new PriorityQueue<HexCoordinates, int>();
            frontier.Enqueue(unit.coordinates, 0);
            costSoFar[unit.coordinates] = 0;

            bool found = false;

            while (frontier.Count > 0)
            {
                HexCoordinates current = frontier.Dequeue();

                if (current == target)
                {
                    found = true;
                    break;
                }

                foreach (HexCoordinates next in current.GetAllNeighbors())
                {
                    TerrainDataSO terrain = getTerrain(next);
                    if (terrain == null) continue;

                    int stepCost = terrain.GetMovementCost(unit.blueprint.movementType);
                    if (stepCost >= TerrainDataSO.IMPASSIBLE) continue;

                    UnitInstance occupant = getUnit(next);
                    if (occupant != null && occupant.teamId != unit.teamId && next != target) continue;

                    int newCost = costSoFar[current] + stepCost;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + HexCoordinates.Distance(next, target);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            if (!found) return null;

            // Yolu yeniden oluştur
            List<HexCoordinates> path = new List<HexCoordinates>();
            HexCoordinates curr = target;
            while (curr != unit.coordinates)
            {
                path.Add(curr);
                curr = cameFrom[curr];
            }
            path.Reverse();
            return path;
        }
    }

    /// <summary>
    /// Mobil A* yol bulma optimizasyonu için hafif öncelik kuyruğu.
    /// </summary>
    public class PriorityQueue<TElement, TPriority> where TPriority : System.IComparable<TPriority>
    {
        private readonly List<(TElement element, TPriority priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(TElement item, TPriority priority)
        {
            elements.Add((item, priority));
            int ci = elements.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (elements[ci].priority.CompareTo(elements[pi].priority) >= 0) break;
                (elements[ci], elements[pi]) = (elements[pi], elements[ci]);
                ci = pi;
            }
        }

        public TElement Dequeue()
        {
            int li = elements.Count - 1;
            var frontItem = elements[0].element;
            elements[0] = elements[li];
            elements.RemoveAt(li);

            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && elements[rc].priority.CompareTo(elements[ci].priority) < 0)
                    ci = rc;
                if (elements[pi].priority.CompareTo(elements[ci].priority) <= 0) break;
                (elements[pi], elements[ci]) = (elements[ci], elements[pi]);
                pi = ci;
            }

            return frontItem;
        }
    }
}
