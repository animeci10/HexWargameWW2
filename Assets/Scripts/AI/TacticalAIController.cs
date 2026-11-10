using System.Collections.Generic;
using HexWargame.Core;
using HexWargame.Data;
using HexWargame.Simulation;
using UnityEngine;

namespace HexWargame.AI
{
    /// <summary>
    /// II. Dünya Savaşı wargame dinamiklerine uygun kural tabanlı Taktiksel Yapay Zeka (AI).
    /// Öncelikler: Hedef şehirleri ele geçirme, zayıf/kuşatılmış birliklere odaklanma, topçu desteği ve siper kazma.
    /// </summary>
    public class TacticalAIController
    {
        private readonly HexGridMap gridMap;
        private readonly HexPathfinder pathfinder;
        private readonly int aiTeamId;

        public TacticalAIController(HexGridMap map, HexPathfinder pathfinder, int aiTeamId = 1)
        {
            this.gridMap = map;
            this.pathfinder = pathfinder;
            this.aiTeamId = aiTeamId;
        }

        public void ExecuteAITurn()
        {
            List<UnitInstance> aiUnits = gridMap.GetUnitsByTeam(aiTeamId);

            foreach (var unit in aiUnits)
            {
                if (unit.currentStrength <= 0) continue;

                // 1. Kritik Hasar Kontrolü: Güç çok azsa siper kaz (Dig-in)
                if (unit.currentStrength <= 3 && unit.blueprint.canDigIn && unit.entrenchmentLevel < 2)
                {
                    unit.DigIn();
                    continue;
                }

                // 2. Mevcut Konumdan Saldırı Fırsatı Kontrolü
                UnitInstance bestTarget = FindBestTargetInRange(unit);
                if (bestTarget != null && !unit.hasAttacked)
                {
                    ExecuteAttack(unit, bestTarget);
                }

                // 3. Hareket Kararı
                if (unit.currentMP > 0 && !unit.isSuppressed)
                {
                    ExecuteMovementAndAdvance(unit);

                    // Hareket ettikten sonra tekrar saldırı menzili kontrolü
                    if (!unit.hasAttacked)
                    {
                        UnitInstance postMoveTarget = FindBestTargetInRange(unit);
                        if (postMoveTarget != null)
                        {
                            ExecuteAttack(unit, postMoveTarget);
                        }
                    }
                }
            }
        }

        private UnitInstance FindBestTargetInRange(UnitInstance unit)
        {
            UnitInstance bestTarget = null;
            float maxScore = float.MinValue;

            foreach (var potentialTarget in gridMap.AllUnits)
            {
                if (potentialTarget.teamId == unit.teamId) continue;
                if (potentialTarget.currentStrength <= 0) continue;

                int dist = HexCoordinates.Distance(unit.coordinates, potentialTarget.coordinates);
                if (dist >= unit.blueprint.attackRangeMin && dist <= unit.blueprint.attackRangeMax)
                {
                    // Hedef puanlama: Canı az olan, zırhı zayıf ve kuşatılmış birlikler önceliklidir
                    float score = (10 - potentialTarget.currentStrength) * 3f;
                    if (potentialTarget.isOutOfSupply) score += 15f;
                    if (potentialTarget.isSuppressed) score += 10f;

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestTarget = potentialTarget;
                    }
                }
            }

            return bestTarget;
        }

        private void ExecuteAttack(UnitInstance attacker, UnitInstance defender)
        {
            TerrainDataSO defTerrain = gridMap.GetTerrain(defender.coordinates);
            var result = CombatEngine.ResolveEngagement(attacker, defender, defTerrain);
            attacker.hasAttacked = true;

            if (defender.currentStrength <= 0)
            {
                gridMap.RemoveUnit(defender);
            }
        }

        private void ExecuteMovementAndAdvance(UnitInstance unit)
        {
            // En yakın oyuncu birimini veya hedef şehri bul
            HexCoordinates bestDestination = unit.coordinates;
            int minDistance = int.MaxValue;

            foreach (var target in gridMap.AllUnits)
            {
                if (target.teamId != unit.teamId && target.currentStrength > 0)
                {
                    int dist = HexCoordinates.Distance(unit.coordinates, target.coordinates);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestDestination = target.coordinates;
                    }
                }
            }

            if (bestDestination == unit.coordinates) return;

            // Birlik için hareket edilebilir tüm hücreleri çek
            var reachable = pathfinder.GetReachableCoordinates(unit);
            HexCoordinates bestMoveHex = unit.coordinates;
            int bestDistToGoal = minDistance;

            foreach (var hex in reachable)
            {
                int dist = HexCoordinates.Distance(hex, bestDestination);
                if (dist < bestDistToGoal)
                {
                    bestDistToGoal = dist;
                    bestMoveHex = hex;
                }
            }

            if (bestMoveHex != unit.coordinates)
            {
                gridMap.MoveUnit(unit, bestMoveHex);
                unit.currentMP = 0; // Basit hareket maliyeti tüketimi
            }
        }
    }
}
