using UnityEngine;

namespace HexWargame.Data
{
    [CreateAssetMenu(fileName = "NewUnitBlueprint", menuName = "Hex Wargame/Unit Blueprint")]
    public class UnitBlueprintSO : ScriptableObject
    {
        [Header("Kimlik ve Sınıflandırma")]
        public string unitId;
        public string displayName; // Örn: "Panzer VI Tiger I", "T-34/85", "Fallschirmjäger"
        public NationFaction faction;
        public int historicalYear = 1943;
        public UnitCategory category;
        public TargetClass targetClass;
        [TextArea] public string historicalDescription;

        [Header("Hareket ve Yakıt")]
        public int maxMovementPoints = 4;
        public MovementType movementType = MovementType.Tracked;
        public int fuelCapacity = 100;
        public int fuelPerHex = 5;
        public bool ignoresZoneOfControl = false;

        [Header("Ateş Gücü & Balistik")]
        public int softAttack = 15;        // Piyadeye karşı güç
        public int hardAttack = 35;        // Zırhlı araçlara karşı güç
        public int penetrationMm = 138;    // Zırh delme kabiliyeti (mm cinsinden)
        public int attackRangeMin = 1;     // Minimum atış menzili (Doğrudan ateş için 1)
        public int attackRangeMax = 1;     // Maksimum menzil (Topçular için 2-4)
        public int suppressionPower = 10;  // Bastırma ve moral kırma gücü
        public int ammoCapacity = 10;      // Cephane stoğu
        public int ammoPerAttack = 1;
        public bool isIndirectFire = false;// Görüş açısı olmadan tepe/orman üzerinden atış

        [Header("Zırh & Korunma")]
        public int armorFrontMm = 100;     // Ön zırh
        public int armorSideMm = 80;       // Yan zırh
        public int armorRearMm = 60;       // Arka zırh
        public int maxStrength = 10;       // Birlik gücü / Can puanı (Klasik 10 basamaklı sistem)
        public int airDefense = 2;         // Uçaksavar savunması
        public int closeDefense = 8;       // Şehir ve ormanda piyade baskınına karşı yakın savunma

        [Header("Özel Taktiksel Yetenekler")]
        public bool canDigIn = true;
        public bool canLayMines = false;
        public bool canBridgeRivers = false;
        public bool hasSmokeDischargers = false;
        public bool canParadrop = false;
    }
}
