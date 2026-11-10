using UnityEngine;

namespace HexWargame.Data
{
    /// <summary>
    /// Test ve prototipleme için tarihsel II. Dünya Savaşı birim şablonlarını oluşturan fabrika sınıfı.
    /// ScriptableObject dosyaları Unity Editörde tek tek oluşturulabileceği gibi, bu sınıf ile kod üzerinden de hızlıca çağrılabilir.
    /// </summary>
    public static class WW2UnitCatalogFactory
    {
        public static UnitBlueprintSO CreateTigerI()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "ger_tiger1";
            bp.displayName = "Panzer VI Tiger I (Ağır Tank)";
            bp.faction = NationFaction.Germany;
            bp.historicalYear = 1943;
            bp.category = UnitCategory.HeavyTank;
            bp.targetClass = TargetClass.HeavyArmor;
            bp.movementType = MovementType.HeavyTracked;
            bp.maxMovementPoints = 3;
            bp.fuelCapacity = 100;
            bp.fuelPerHex = 12;

            // 8.8 cm KwK 36 L/56
            bp.softAttack = 22;
            bp.hardAttack = 42;
            bp.penetrationMm = 138;
            bp.attackRangeMin = 1;
            bp.attackRangeMax = 2;
            bp.suppressionPower = 15;
            bp.ammoCapacity = 8;

            bp.armorFrontMm = 102;
            bp.armorSideMm = 82;
            bp.armorRearMm = 62;
            bp.maxStrength = 10;
            bp.closeDefense = 6;
            return bp;
        }

        public static UnitBlueprintSO CreateT34_85()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "sov_t34_85";
            bp.displayName = "T-34/85 (Orta Tank)";
            bp.faction = NationFaction.USSR;
            bp.historicalYear = 1944;
            bp.category = UnitCategory.MediumTank;
            bp.targetClass = TargetClass.MediumArmor;
            bp.movementType = MovementType.Tracked;
            bp.maxMovementPoints = 5;
            bp.fuelCapacity = 120;
            bp.fuelPerHex = 8;

            // 85 mm ZiS-S-53
            bp.softAttack = 18;
            bp.hardAttack = 34;
            bp.penetrationMm = 125;
            bp.attackRangeMin = 1;
            bp.attackRangeMax = 1;
            bp.suppressionPower = 10;
            bp.ammoCapacity = 10;

            bp.armorFrontMm = 90; // Eğimli efektif kalınlık
            bp.armorSideMm = 45;
            bp.armorRearMm = 45;
            bp.maxStrength = 10;
            bp.closeDefense = 7;
            return bp;
        }

        public static UnitBlueprintSO CreateShermanEasyEight()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "usa_m4a3e8";
            bp.displayName = "M4A3E8 Sherman 'Easy Eight'";
            bp.faction = NationFaction.USA;
            bp.historicalYear = 1944;
            bp.category = UnitCategory.MediumTank;
            bp.targetClass = TargetClass.MediumArmor;
            bp.movementType = MovementType.Tracked;
            bp.maxMovementPoints = 5;
            bp.fuelCapacity = 110;
            bp.fuelPerHex = 7;

            // 76 mm M1 Gun
            bp.softAttack = 20;
            bp.hardAttack = 32;
            bp.penetrationMm = 120;
            bp.attackRangeMin = 1;
            bp.attackRangeMax = 1;
            bp.suppressionPower = 12;
            bp.ammoCapacity = 12;

            bp.armorFrontMm = 88;
            bp.armorSideMm = 38;
            bp.armorRearMm = 38;
            bp.maxStrength = 10;
            bp.closeDefense = 8;
            return bp;
        }

        public static UnitBlueprintSO CreateKatyusha()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "sov_bm13_katyusha";
            bp.displayName = "BM-13 Katyusha (Roket Topçusu)";
            bp.faction = NationFaction.USSR;
            bp.historicalYear = 1942;
            bp.category = UnitCategory.RocketArtillery;
            bp.targetClass = TargetClass.LightVehicle;
            bp.movementType = MovementType.Wheeled;
            bp.maxMovementPoints = 6;
            bp.fuelCapacity = 140;
            bp.fuelPerHex = 6;

            // 132 mm M-13 Roket Bataryası
            bp.softAttack = 40;
            bp.hardAttack = 12;
            bp.penetrationMm = 30;
            bp.attackRangeMin = 2;
            bp.attackRangeMax = 4;
            bp.suppressionPower = 35; // Devasa bastırma etkisi
            bp.ammoCapacity = 4;
            bp.isIndirectFire = true;

            bp.armorFrontMm = 10;
            bp.armorSideMm = 6;
            bp.armorRearMm = 6;
            bp.maxStrength = 10;
            bp.closeDefense = 2;
            return bp;
        }

        public static UnitBlueprintSO CreateFlak88()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "ger_flak88";
            bp.displayName = "8.8 cm FlaK 36 (Çift Rollü Ağır Top)";
            bp.faction = NationFaction.Germany;
            bp.historicalYear = 1941;
            bp.category = UnitCategory.AntiAirGun;
            bp.targetClass = TargetClass.SoftInfantry;
            bp.movementType = MovementType.Wheeled;
            bp.maxMovementPoints = 2;
            bp.fuelCapacity = 0; // Çekili batarya

            // Hem hava hem ağır zırh avcısı
            bp.softAttack = 25;
            bp.hardAttack = 46;
            bp.penetrationMm = 142;
            bp.attackRangeMin = 1;
            bp.attackRangeMax = 3;
            bp.suppressionPower = 20;
            bp.ammoCapacity = 8;
            bp.airDefense = 45;

            bp.armorFrontMm = 15; // Top kalkanı
            bp.armorSideMm = 0;
            bp.armorRearMm = 0;
            bp.maxStrength = 10;
            bp.closeDefense = 4;
            return bp;
        }

        public static UnitBlueprintSO CreateFallschirmjager()
        {
            var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
            bp.unitId = "ger_fallschirmjager";
            bp.displayName = "Fallschirmjäger (Seçkin Hava İndirme)";
            bp.faction = NationFaction.Germany;
            bp.historicalYear = 1941;
            bp.category = UnitCategory.Paratroopers;
            bp.targetClass = TargetClass.SoftInfantry;
            bp.movementType = MovementType.Foot;
            bp.maxMovementPoints = 3;
            bp.fuelCapacity = 0;
            bp.canParadrop = true;

            bp.softAttack = 24;
            bp.hardAttack = 14;
            bp.penetrationMm = 45; // Panzerfaust donanımı
            bp.attackRangeMin = 1;
            bp.attackRangeMax = 1;
            bp.suppressionPower = 15;
            bp.ammoCapacity = 8;

            bp.armorFrontMm = 0;
            bp.armorSideMm = 0;
            bp.armorRearMm = 0;
            bp.maxStrength = 10;
            bp.closeDefense = 14;
            bp.canDigIn = true;
            return bp;
        }
    }
}
