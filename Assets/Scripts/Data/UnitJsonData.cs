using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexWargame.Data
{
    [Serializable]
    public class UnitJsonData
    {
        public string unitId;
        public string displayName;
        public string faction;
        public int historicalYear = 1943;
        public string category;
        public string targetClass;
        public string description;

        // Hareket & Yakıt
        public int maxMP = 4;
        public string movementType = "Tracked";
        public int fuelCapacity = 100;
        public int fuelPerHex = 5;

        // Balistik & Ateş Gücü
        public int softAttack = 15;
        public int hardAttack = 30;
        public int penetrationMm = 100;
        public int attackRangeMin = 1;
        public int attackRangeMax = 1;
        public int suppressionPower = 10;
        public int ammoCapacity = 10;
        public bool isIndirectFire = false;

        // Zırh & Dayanıklılık
        public int armorFrontMm = 80;
        public int armorSideMm = 50;
        public int armorRearMm = 40;
        public int maxStrength = 10;
        public int airDefense = 2;
        public int closeDefense = 6;

        // Yetenekler
        public bool canDigIn = true;
        public bool canParadrop = false;
    }

    [Serializable]
    public class UnitCatalogJsonWrapper
    {
        public string version = "1.0.0";
        public List<UnitJsonData> units = new();
    }

    /// <summary>
    /// Harici JSON dosyalarından birim kataloglarını yükleyen ve çalışma zamanında ScriptableObject'e dönüştüren sınıf.
    /// Yeni birimler ve denge güncellemeleri kod derlemeye gerek kalmadan anında oyuna eklenebilir.
    /// </summary>
    public static class UnitDataLoader
    {
        public static List<UnitBlueprintSO> LoadUnitsFromJson(string jsonText)
        {
            var wrapper = JsonUtility.FromJson<UnitCatalogJsonWrapper>(jsonText);
            List<UnitBlueprintSO> blueprints = new();

            if (wrapper == null || wrapper.units == null) return blueprints;

            foreach (var data in wrapper.units)
            {
                var bp = ScriptableObject.CreateInstance<UnitBlueprintSO>();
                bp.unitId = data.unitId;
                bp.displayName = data.displayName;
                bp.historicalYear = data.historicalYear;
                bp.historicalDescription = data.description;

                // Enum dönüşümleri
                Enum.TryParse(data.faction, true, out bp.faction);
                Enum.TryParse(data.category, true, out bp.category);
                Enum.TryParse(data.targetClass, true, out bp.targetClass);
                Enum.TryParse(data.movementType, true, out bp.movementType);

                bp.maxMovementPoints = data.maxMP;
                bp.fuelCapacity = data.fuelCapacity;
                bp.fuelPerHex = data.fuelPerHex;

                bp.softAttack = data.softAttack;
                bp.hardAttack = data.hardAttack;
                bp.penetrationMm = data.penetrationMm;
                bp.attackRangeMin = data.attackRangeMin;
                bp.attackRangeMax = data.attackRangeMax;
                bp.suppressionPower = data.suppressionPower;
                bp.ammoCapacity = data.ammoCapacity;
                bp.isIndirectFire = data.isIndirectFire;

                bp.armorFrontMm = data.armorFrontMm;
                bp.armorSideMm = data.armorSideMm;
                bp.armorRearMm = data.armorRearMm;
                bp.maxStrength = data.maxStrength;
                bp.airDefense = data.airDefense;
                bp.closeDefense = data.closeDefense;

                bp.canDigIn = data.canDigIn;
                bp.canParadrop = data.canParadrop;

                blueprints.Add(bp);
            }

            return blueprints;
        }

        public static string ExportUnitsToJson(List<UnitBlueprintSO> blueprints, string version = "1.0.0")
        {
            var wrapper = new UnitCatalogJsonWrapper { version = version };

            foreach (var bp in blueprints)
            {
                wrapper.units.Add(new UnitJsonData
                {
                    unitId = bp.unitId,
                    displayName = bp.displayName,
                    faction = bp.faction.ToString(),
                    historicalYear = bp.historicalYear,
                    category = bp.category.ToString(),
                    targetClass = bp.targetClass.ToString(),
                    description = bp.historicalDescription,
                    maxMP = bp.maxMovementPoints,
                    movementType = bp.movementType.ToString(),
                    fuelCapacity = bp.fuelCapacity,
                    fuelPerHex = bp.fuelPerHex,
                    softAttack = bp.softAttack,
                    hardAttack = bp.hardAttack,
                    penetrationMm = bp.penetrationMm,
                    attackRangeMin = bp.attackRangeMin,
                    attackRangeMax = bp.attackRangeMax,
                    suppressionPower = bp.suppressionPower,
                    ammoCapacity = bp.ammoCapacity,
                    isIndirectFire = bp.isIndirectFire,
                    armorFrontMm = bp.armorFrontMm,
                    armorSideMm = bp.armorSideMm,
                    armorRearMm = bp.armorRearMm,
                    maxStrength = bp.maxStrength,
                    airDefense = bp.airDefense,
                    closeDefense = bp.closeDefense,
                    canDigIn = bp.canDigIn,
                    canParadrop = bp.canParadrop
                });
            }

            return JsonUtility.ToJson(wrapper, true);
        }
    }
}
