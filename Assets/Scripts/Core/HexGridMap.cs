using System;
using System.Collections.Generic;
using HexWargame.Data;
using HexWargame.Simulation;
using UnityEngine;

namespace HexWargame.Core
{
    [Serializable]
    public class HexCellData
    {
        public HexCoordinates coordinates;
        public TerrainDataSO terrain;
        public int elevation = 0;
        public bool hasRoad = false;
        public bool isRiverEdge = false;
        public bool isObjectiveCity = false;
        public int objectivePoints = 0;
        public int ownerTeam = -1; // -1 = Nötr, 0 = Müttefikler, 1 = Mihver vb.
    }

    /// <summary>
    /// Hex haritasını ve üzerindeki birimlerin/arazilerin anlık durumunu yöneten çekirdek veri yapısı.
    /// </summary>
    public class HexGridMap
    {
        private readonly Dictionary<HexCoordinates, HexCellData> cells = new();
        private readonly Dictionary<HexCoordinates, UnitInstance> unitsByPosition = new();
        private readonly Dictionary<string, UnitInstance> unitsById = new();

        public int CellCount => cells.Count;
        public IEnumerable<HexCellData> AllCells => cells.Values;
        public IEnumerable<UnitInstance> AllUnits => unitsById.Values;

        public void AddCell(HexCellData cell)
        {
            cells[cell.coordinates] = cell;
        }

        public HexCellData GetCell(HexCoordinates coords)
        {
            cells.TryGetValue(coords, out var cell);
            return cell;
        }

        public bool Contains(HexCoordinates coords) => cells.ContainsKey(coords);

        public TerrainDataSO GetTerrain(HexCoordinates coords)
        {
            return cells.TryGetValue(coords, out var cell) ? cell.terrain : null;
        }

        #region Birlik Yerleşimi ve Takibi

        public bool PlaceUnit(UnitInstance unit, HexCoordinates coords)
        {
            if (!cells.ContainsKey(coords)) return false;
            if (unitsByPosition.ContainsKey(coords)) return false;

            unit.coordinates = coords;
            unitsByPosition[coords] = unit;
            unitsById[unit.instanceId] = unit;
            return true;
        }

        public bool MoveUnit(UnitInstance unit, HexCoordinates newCoords)
        {
            if (!cells.ContainsKey(newCoords)) return false;
            if (unitsByPosition.ContainsKey(newCoords) && unitsByPosition[newCoords] != unit) return false;

            unitsByPosition.Remove(unit.coordinates);
            unit.coordinates = newCoords;
            unitsByPosition[newCoords] = unit;
            return true;
        }

        public void RemoveUnit(UnitInstance unit)
        {
            unitsByPosition.Remove(unit.coordinates);
            unitsById.Remove(unit.instanceId);
        }

        public UnitInstance GetUnitAt(HexCoordinates coords)
        {
            unitsByPosition.TryGetValue(coords, out var unit);
            return unit;
        }

        public UnitInstance GetUnitById(string unitId)
        {
            unitsById.TryGetValue(unitId, out var unit);
            return unit;
        }

        public List<UnitInstance> GetUnitsByTeam(int teamId)
        {
            List<UnitInstance> teamUnits = new();
            foreach (var u in unitsById.Values)
            {
                if (u.teamId == teamId) teamUnits.Add(u);
            }
            return teamUnits;
        }

        public void Clear()
        {
            cells.Clear();
            unitsByPosition.Clear();
            unitsById.Clear();
        }

        #endregion
    }
}
