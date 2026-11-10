using System.Collections.Generic;
using HexWargame.AI;
using HexWargame.Data;
using HexWargame.InputControl;
using HexWargame.Simulation;
using UnityEngine;

namespace HexWargame.Core
{
    /// <summary>
    /// Savaş alanındaki tüm sistemleri (Harita, Birimler, Yol Bulma, Savaş, İkmal, AI ve Kamera) birbirine bağlayan ana orkestratör.
    /// </summary>
    public class BattlefieldManager : MonoBehaviour
    {
        [Header("Bileşen Referansları")]
        [SerializeField] private MobileHexCameraController cameraController;

        [Header("Harita Boyutları")]
        [SerializeField] private int mapWidth = 20;
        [SerializeField] private int mapHeight = 15;
        [SerializeField] private float hexRadius = 1.0f;

        [Header("Hazır Arazi Tanımları")]
        [SerializeField] private TerrainDataSO plainsTerrain;
        [SerializeField] private TerrainDataSO forestTerrain;
        [SerializeField] private TerrainDataSO hillsTerrain;
        [SerializeField] private TerrainDataSO cityTerrain;

        // Çekirdek Sistemler
        public HexGridMap GridMap { get; private set; }
        public HexPathfinder Pathfinder { get; private set; }
        public SupplyEngine Supply { get; private set; }
        public TurnManager Turns { get; private set; }
        public TacticalAIController AI { get; private set; }

        // Seçim Durumu
        private UnitInstance selectedUnit = null;
        private HashSet<HexCoordinates> currentReachableHexes = new();

        private void Awake()
        {
            InitializeBattlefield();
        }

        private void Start()
        {
            if (cameraController != null)
            {
                cameraController.OnHexTapped += HandleHexTapped;
            }

            Turns.OnPhaseChanged += HandlePhaseChanged;
            Turns.StartTurnCycle();
        }

        private void OnDestroy()
        {
            if (cameraController != null)
            {
                cameraController.OnHexTapped -= HandleHexTapped;
            }
        }

        private void InitializeBattlefield()
        {
            GridMap = new HexGridMap();

            // 1. Basit Test Haritası Oluştur
            for (int r = 0; r < mapHeight; r++)
            {
                int rOffset = r >> 1;
                for (int q = -rOffset; q < mapWidth - rOffset; q++)
                {
                    HexCoordinates coords = new HexCoordinates(q, r);
                    TerrainDataSO chosenTerrain = plainsTerrain;

                    // Rastgele orman ve tepe dağılımı (Fallback)
                    if ((q + r) % 5 == 0) chosenTerrain = forestTerrain;
                    else if ((q * 3 + r) % 7 == 0) chosenTerrain = hillsTerrain;

                    GridMap.AddCell(new HexCellData
                    {
                        coordinates = coords,
                        terrain = chosenTerrain,
                        elevation = (chosenTerrain == hillsTerrain) ? 1 : 0
                    });
                }
            }

            // 2. Alt Sistemleri Kur
            Pathfinder = new HexPathfinder(
                coords => GridMap.GetTerrain(coords),
                coords => GridMap.GetUnitAt(coords)
            );

            Supply = new SupplyEngine((coords, teamId) =>
            {
                UnitInstance u = GridMap.GetUnitAt(coords);
                return u != null && u.teamId != teamId;
            });

            Turns = new TurnManager(GridMap, Supply, 30);
            AI = new TacticalAIController(GridMap, Pathfinder, 1);

            // 3. Varsayılan Başlangıç Birliklerini Yerleştir
            SpawnInitialScenarioUnits();
        }

        private void SpawnInitialScenarioUnits()
        {
            // Oyuncu (Takım 0) Birlikleri
            var tiger = WW2UnitCatalogFactory.CreateTigerI();
            var grenadiers = WW2UnitCatalogFactory.CreateFallschirmjager();
            GridMap.PlaceUnit(new UnitInstance("p_tiger_1", tiger, new HexCoordinates(2, 3), 0), new HexCoordinates(2, 3));
            GridMap.PlaceUnit(new UnitInstance("p_inf_1", grenadiers, new HexCoordinates(3, 3), 0), new HexCoordinates(3, 3));

            // Düşman (Takım 1) Birlikleri
            var t34 = WW2UnitCatalogFactory.CreateT34_85();
            var katyusha = WW2UnitCatalogFactory.CreateKatyusha();
            GridMap.PlaceUnit(new UnitInstance("e_t34_1", t34, new HexCoordinates(8, 7), 1), new HexCoordinates(8, 7));
            GridMap.PlaceUnit(new UnitInstance("e_katyusha_1", katyusha, new HexCoordinates(9, 8), 1), new HexCoordinates(9, 8));

            // İkmal Depoları Kaydı
            Turns.RegisterSupplyDepot(new HexCoordinates(0, 0), 0);
            Turns.RegisterSupplyDepot(new HexCoordinates(15, 12), 1);
        }

        private void HandleHexTapped(HexCoordinates tappedCoords)
        {
            if (Turns.CurrentPhase != GamePhase.PlayerPhase) return;

            UnitInstance unitAtHex = GridMap.GetUnitAt(tappedCoords);

            // 1. Dost Birim Seçimi
            if (unitAtHex != null && unitAtHex.teamId == 0)
            {
                SelectUnit(unitAtHex);
                return;
            }

            // 2. Birim Seçiliyken Hedefe Tıklama (Hareket veya Saldırı)
            if (selectedUnit != null)
            {
                // Düşmana Tıklanmışsa Saldır
                if (unitAtHex != null && unitAtHex.teamId != 0)
                {
                    TryAttackTarget(selectedUnit, unitAtHex);
                    return;
                }

                // Boş Hücreye Tıklanmışsa ve Menzildeyse Hareket Et
                if (currentReachableHexes.Contains(tappedCoords))
                {
                    GridMap.MoveUnit(selectedUnit, tappedCoords);
                    selectedUnit.currentMP = 0; // Kalan MP tüketildi
                    Deselect();
                    return;
                }
            }

            // Boş alana tıklanınca seçimi kaldır
            Deselect();
        }

        private void SelectUnit(UnitInstance unit)
        {
            selectedUnit = unit;
            currentReachableHexes = Pathfinder.GetReachableCoordinates(unit);
            Debug.Log($"Seçilen Birlik: {unit.blueprint.displayName} | Can: {unit.currentStrength} | MP: {unit.currentMP}");
        }

        private void Deselect()
        {
            selectedUnit = null;
            currentReachableHexes.Clear();
        }

        private void TryAttackTarget(UnitInstance attacker, UnitInstance defender)
        {
            if (attacker.hasAttacked)
            {
                Debug.LogWarning("Bu birlik bu tur zaten saldırdı!");
                return;
            }

            int dist = HexCoordinates.Distance(attacker.coordinates, defender.coordinates);
            if (dist < attacker.blueprint.attackRangeMin || dist > attacker.blueprint.attackRangeMax)
            {
                Debug.LogWarning("Hedef atış menzili dışında!");
                return;
            }

            TerrainDataSO defTerrain = GridMap.GetTerrain(defender.coordinates);
            var result = CombatEngine.ResolveEngagement(attacker, defender, defTerrain);
            attacker.hasAttacked = true;

            Debug.Log($"Çatışma Sonucu: {result.outcome} | Verilen Hasar: {result.defenderDamageTaken} | Alınan Hasar: {result.attackerDamageTaken}");

            if (defender.currentStrength <= 0)
            {
                GridMap.RemoveUnit(defender);
                Debug.Log("Düşman birimi imha edildi!");
            }

            Deselect();
        }

        private void HandlePhaseChanged(int turn, GamePhase phase)
        {
            Debug.Log($"Tur: {turn} | Faz: {phase}");

            if (phase == GamePhase.AIPhase)
            {
                AI.ExecuteAITurn();
                Turns.FinishAIPhase();
            }
        }

        #region UI Buton Aksiyonları

        public void OnClickEndTurn()
        {
            Deselect();
            Turns.EndPlayerPhase();
        }

        public void OnClickDigIn()
        {
            if (selectedUnit != null && selectedUnit.teamId == 0)
            {
                selectedUnit.DigIn();
                Deselect();
            }
        }

        #endregion
    }
}
