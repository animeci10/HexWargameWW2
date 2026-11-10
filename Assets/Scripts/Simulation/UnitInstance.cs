using System;
using HexWargame.Core;
using HexWargame.Data;
using UnityEngine;

namespace HexWargame.Simulation
{
    [Serializable]
    public class UnitInstance
    {
        public string instanceId;
        public UnitBlueprintSO blueprint;
        public HexCoordinates coordinates;
        public int teamId; // 0 = Oyuncu / Müttefikler, 1 = Düşman / Mihver vb.

        [Header("Dinamik Durum Değişkenleri")]
        public int currentStrength;       // Mevcut güç / Can (Örn: 10 üzerinden)
        public int currentMP;             // Kalan Hareket Puanı
        public int currentFuel;           // Kalan Yakıt
        public int currentAmmo;           // Kalan Mühimmat
        public int currentMorale = 100;   // Moral (0 - 100)
        public int entrenchmentLevel = 0; // Mevcut siper derinliği
        public int facingDirectionIndex = 0; // Baktığı yön (0 - 5)
        public int veterancyStars = 0;    // Kıdem/Tecrübe (0 - 5 Yıldız)
        public bool isSuppressed = false; // Bastırıldı mı?
        public bool hasAttacked = false;  // Bu tur saldırdı mı?
        public bool isOutOfSupply = false;// İkmal hattı kesildi mi?

        public UnitInstance(string id, UnitBlueprintSO bp, HexCoordinates startCoords, int team)
        {
            instanceId = id;
            blueprint = bp;
            coordinates = startCoords;
            teamId = team;

            currentStrength = bp.maxStrength;
            currentMP = bp.maxMovementPoints;
            currentFuel = bp.fuelCapacity;
            currentAmmo = bp.ammoCapacity;
            currentMorale = 100;
            entrenchmentLevel = 0;
            facingDirectionIndex = 0;
            veterancyStars = 0;
            isSuppressed = false;
            hasAttacked = false;
            isOutOfSupply = false;
        }

        public void StartNewTurn()
        {
            hasAttacked = false;
            isSuppressed = false;

            if (isOutOfSupply)
            {
                // Kuşatma altındaysa hareket puanı ve moral kaybı
                currentMP = Mathf.Max(0, blueprint.maxMovementPoints / 2);
                currentMorale = Mathf.Max(10, currentMorale - 20);
            }
            else
            {
                currentMP = (currentFuel <= 0 && blueprint.fuelCapacity > 0) ? 0 : blueprint.maxMovementPoints;
                currentMorale = Mathf.Min(100, currentMorale + 10);
            }
        }

        public void DigIn()
        {
            if (blueprint.canDigIn && currentMP >= 1)
            {
                entrenchmentLevel++;
                currentMP = 0;
            }
        }

        public int GetEffectiveArmor(HitAngle angle)
        {
            return angle switch
            {
                HitAngle.Front => blueprint.armorFrontMm,
                HitAngle.Flank => Mathf.RoundToInt(blueprint.armorSideMm * 0.85f),
                HitAngle.Rear => Mathf.RoundToInt(blueprint.armorRearMm * 0.65f),
                _ => blueprint.armorFrontMm
            };
        }

        public void ApplyCombatResults(int strengthDamage, int suppressionAmount, bool forcedRetreat)
        {
            currentStrength = Mathf.Max(0, currentStrength - strengthDamage);
            currentMorale = Mathf.Max(0, currentMorale - suppressionAmount);

            if (suppressionAmount >= 20 || currentMorale < 40)
            {
                isSuppressed = true;
            }

            // Hasar alınca siper seviyesi aşınır
            if (strengthDamage > 0 && entrenchmentLevel > 0)
            {
                entrenchmentLevel--;
            }
        }
    }
}
