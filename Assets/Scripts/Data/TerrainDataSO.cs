using System;
using UnityEngine;

namespace HexWargame.Data
{
    [CreateAssetMenu(fileName = "NewTerrainData", menuName = "Hex Wargame/Terrain Data")]
    public class TerrainDataSO : ScriptableObject
    {
        [Header("Tanımlama")]
        public string terrainId;
        public string displayName;
        public Color mapColor = Color.green;

        [Header("Taktiksel Değerler")]
        [Range(0f, 1f)] public float defenseMultiplier = 0.0f; // 0.3 = %30 Savunma Bonusu
        [Range(0f, 1f)] public float coverMultiplier = 0.0f;   // Topçu ve hava saldırısı yumuşatma
        public int maxEntrenchmentLevel = 3;                   // Maksimum siper seviyesi
        public bool blocksLineOfSight = false;                 // Görüş hattını keser mi?
        public bool isWaterOrMarsh = false;

        [Header("Hareket Puanı Maliyetleri (Movement Costs)")]
        public int costFoot = 1;
        public int costMountainFoot = 1;
        public int costWheeled = 2;
        public int costHalfTrack = 2;
        public int costTracked = 1;
        public int costHeavyTracked = 2;
        public int costAmphibious = 1;

        public const int IMPASSIBLE = 999;

        public int GetMovementCost(MovementType moveType)
        {
            return moveType switch
            {
                MovementType.Foot => costFoot,
                MovementType.MountainFoot => costMountainFoot,
                MovementType.Wheeled => costWheeled,
                MovementType.HalfTrack => costHalfTrack,
                MovementType.Tracked => costTracked,
                MovementType.HeavyTracked => costHeavyTracked,
                MovementType.Amphibious => costAmphibious,
                MovementType.Airborne => 1,
                _ => 1
            };
        }
    }
}
