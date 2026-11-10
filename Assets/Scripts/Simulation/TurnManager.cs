using System;
using System.Collections.Generic;
using HexWargame.Core;
using HexWargame.Data;
using UnityEngine;

namespace HexWargame.Simulation
{
    public enum GamePhase
    {
        LogisticsPhase,
        PlayerPhase,
        AIPhase,
        TurnEndEvaluation,
        GameOver
    }

    public enum VictoryState
    {
        Ongoing,
        PlayerDecisiveVictory,
        PlayerMarginalVictory,
        PlayerDefeat
    }

    public class TurnManager
    {
        public int CurrentTurn { get; private set; } = 1;
        public int MaxTurns { get; private set; } = 25;
        public GamePhase CurrentPhase { get; private set; } = GamePhase.PlayerPhase;
        public WeatherType CurrentWeather { get; private set; } = WeatherType.ClearDry;

        public event Action<int, GamePhase> OnPhaseChanged;
        public event Action<VictoryState> OnGameOver;

        private readonly HexGridMap gridMap;
        private readonly SupplyEngine supplyEngine;
        private readonly List<HexCoordinates> friendlySupplyDepots = new();
        private readonly List<HexCoordinates> enemySupplyDepots = new();

        public TurnManager(HexGridMap map, SupplyEngine supply, int maxTurns = 25)
        {
            this.gridMap = map;
            this.supplyEngine = supply;
            this.MaxTurns = maxTurns;
        }

        public void RegisterSupplyDepot(HexCoordinates coords, int teamId)
        {
            if (teamId == 0) friendlySupplyDepots.Add(coords);
            else enemySupplyDepots.Add(coords);
        }

        public void StartTurnCycle()
        {
            CurrentTurn = 1;
            BeginLogisticsPhase();
        }

        public void EndPlayerPhase()
        {
            if (CurrentPhase != GamePhase.PlayerPhase) return;
            BeginAIPhase();
        }

        private void BeginLogisticsPhase()
        {
            CurrentPhase = GamePhase.LogisticsPhase;
            OnPhaseChanged?.Invoke(CurrentTurn, CurrentPhase);

            // 1. İkmal ve Kuşatma Hatlarını Hesapla
            List<UnitInstance> allUnits = new(gridMap.AllUnits);
            supplyEngine.EvaluateSupplyNetwork(allUnits, friendlySupplyDepots, 12);

            // 2. Birliklerin Tur Başı Değişkenlerini Yenile
            foreach (var unit in allUnits)
            {
                unit.StartNewTurn();
            }

            // Doğrudan Oyuncu Fazına Geç
            CurrentPhase = GamePhase.PlayerPhase;
            OnPhaseChanged?.Invoke(CurrentTurn, CurrentPhase);
        }

        private void BeginAIPhase()
        {
            CurrentPhase = GamePhase.AIPhase;
            OnPhaseChanged?.Invoke(CurrentTurn, CurrentPhase);
            // AI hamleleri tamamlandığında EvaluateTurnEnd çağrılır
        }

        public void FinishAIPhase()
        {
            EvaluateTurnEnd();
        }

        private void EvaluateTurnEnd()
        {
            CurrentPhase = GamePhase.TurnEndEvaluation;
            OnPhaseChanged?.Invoke(CurrentTurn, CurrentPhase);

            // Zafer & Yenilgi Koşullarını Denetle
            var victory = CheckVictoryConditions();
            if (victory != VictoryState.Ongoing)
            {
                CurrentPhase = GamePhase.GameOver;
                OnGameOver?.Invoke(victory);
                return;
            }

            // Yeni Tura Geç
            CurrentTurn++;
            if (CurrentTurn > MaxTurns)
            {
                CurrentPhase = GamePhase.GameOver;
                OnGameOver?.Invoke(VictoryState.PlayerDefeat);
                return;
            }

            BeginLogisticsPhase();
        }

        private VictoryState CheckVictoryConditions()
        {
            int playerUnits = 0;
            int enemyUnits = 0;
            int playerControlledObjectives = 0;
            int totalObjectives = 0;

            foreach (var unit in gridMap.AllUnits)
            {
                if (unit.teamId == 0) playerUnits++;
                else enemyUnits++;
            }

            foreach (var cell in gridMap.AllCells)
            {
                if (cell.isObjectiveCity)
                {
                    totalObjectives++;
                    if (cell.ownerTeam == 0) playerControlledObjectives++;
                }
            }

            if (playerUnits == 0) return VictoryState.PlayerDefeat;
            if (enemyUnits == 0) return VictoryState.PlayerDecisiveVictory;

            if (totalObjectives > 0 && playerControlledObjectives == totalObjectives)
            {
                return VictoryState.PlayerDecisiveVictory;
            }

            return VictoryState.Ongoing;
        }
    }
}
