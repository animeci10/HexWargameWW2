using HexWargame.Core;
using HexWargame.Data;
using UnityEngine;

namespace HexWargame.Simulation
{
    public struct CombatResult
    {
        public AttackResultType outcome;
        public int attackerDamageTaken;
        public int defenderDamageTaken;
        public int defenderSuppression;
        public bool didDefenderCounterAttack;
        public HitAngle hitAngle;
        public float penetrationRatio;
    }

    public static class CombatEngine
    {
        /// <summary>
        /// İki birlik arasındaki çatışmayı tam balistik, zırh, siper ve açı hesaplamalarıyla çözer.
        /// </summary>
        public static CombatResult ResolveEngagement(
            UnitInstance attacker, 
            UnitInstance defender, 
            TerrainDataSO defenderTerrain,
            int attackerElevation = 0,
            int defenderElevation = 0)
        {
            CombatResult result = new CombatResult();

            int dist = HexCoordinates.Distance(attacker.coordinates, defender.coordinates);
            if (dist < attacker.blueprint.attackRangeMin || dist > attacker.blueprint.attackRangeMax)
            {
                result.outcome = AttackResultType.DeflectedBounced;
                return result;
            }

            // Mühimmat kontrolü
            if (attacker.currentAmmo <= 0)
            {
                result.outcome = AttackResultType.DeflectedBounced;
                return result;
            }
            attacker.currentAmmo = Mathf.Max(0, attacker.currentAmmo - attacker.blueprint.ammoPerAttack);

            // Vuruş Açısını Hesapla (Ön, Yan, Arka)
            result.hitAngle = CalculateHitAngle(attacker.coordinates, defender.coordinates, defender.facingDirectionIndex);

            // Zırh Delme (Penetration) Oranı
            int effectiveArmor = defender.GetEffectiveArmor(result.hitAngle);
            result.penetrationRatio = (effectiveArmor > 0) 
                ? (float)attacker.blueprint.penetrationMm / effectiveArmor 
                : 2.0f;

            // Temel Saldırı Gücünü Belirle (Yumuşak vs Zırhlı)
            float baseAttack = (defender.blueprint.targetClass == TargetClass.SoftInfantry) 
                ? attacker.blueprint.softAttack 
                : attacker.blueprint.hardAttack;

            // Birliğin mevcut gücü ve tecrübesi (Veterancy) çarpanı
            float strengthFactor = (float)attacker.currentStrength / attacker.blueprint.maxStrength;
            float veterancyBonus = 1.0f + (attacker.veterancyStars * 0.08f);
            float finalPower = baseAttack * strengthFactor * veterancyBonus;

            // Yüksek zemin avantajı (+%20 hasar)
            if (attackerElevation > defenderElevation)
            {
                finalPower *= 1.20f;
            }

            // Balistik Delme Kontrolü
            bool isArmored = defender.blueprint.targetClass != TargetClass.SoftInfantry;
            float damageMultiplier = 1.0f;

            if (isArmored)
            {
                if (result.penetrationRatio < 0.65f)
                {
                    // Zırh mermiyi sektirdi! (Ricochet / Deflection)
                    result.outcome = AttackResultType.DeflectedBounced;
                    damageMultiplier = 0.05f; // Sadece optik veya palet çizikleri
                }
                else if (result.penetrationRatio < 1.0f)
                {
                    // Kısmi delme (Spalling / Çatlak)
                    damageMultiplier = 0.45f;
                    result.outcome = AttackResultType.LightDamage;
                }
                else
                {
                    // Tam Zırh Delme (Penetration)
                    damageMultiplier = 1.0f + (result.penetrationRatio - 1.0f) * 0.25f;
                    result.outcome = AttackResultType.HeavyDamage;
                }
            }

            // Savunma ve Siper İndirimi
            float terrainDefense = defenderTerrain != null ? defenderTerrain.defenseMultiplier : 0f;
            float entrenchmentDefense = defender.entrenchmentLevel * 0.12f;
            float totalDefenseMod = 1.0f - Mathf.Clamp(terrainDefense + entrenchmentDefense, 0f, 0.75f);

            int defenderDamage = Mathf.RoundToInt(finalPower * damageMultiplier * totalDefenseMod * 0.2f);
            defenderDamage = Mathf.Max(damageMultiplier > 0.1f ? 1 : 0, defenderDamage);

            // Bastırma ve Moral Etkisi
            int suppression = Mathf.RoundToInt(attacker.blueprint.suppressionPower * (1.0f - (defenderTerrain != null ? defenderTerrain.coverMultiplier : 0f)));

            // Hasarı Uygula
            defender.ApplyCombatResults(defenderDamage, suppression, false);
            result.defenderDamageTaken = defenderDamage;
            result.defenderSuppression = suppression;

            if (defender.currentStrength <= 0)
            {
                result.outcome = AttackResultType.TargetDestroyed;
                return result;
            }

            // Karşı Saldırı (Counter-Attack) Kontrolü
            bool canCounter = dist >= defender.blueprint.attackRangeMin &&
                              dist <= defender.blueprint.attackRangeMax &&
                              !defender.isSuppressed &&
                              defender.currentAmmo > 0;

            if (canCounter)
            {
                result.didDefenderCounterAttack = true;
                defender.currentAmmo = Mathf.Max(0, defender.currentAmmo - defender.blueprint.ammoPerAttack);

                float defBase = (attacker.blueprint.targetClass == TargetClass.SoftInfantry) 
                    ? defender.blueprint.softAttack 
                    : defender.blueprint.hardAttack;

                float defStrengthFactor = (float)defender.currentStrength / defender.blueprint.maxStrength;
                int attackerDamage = Mathf.RoundToInt(defBase * defStrengthFactor * 0.15f);
                attackerDamage = Mathf.Max(1, attackerDamage);

                attacker.ApplyCombatResults(attackerDamage, 5, false);
                result.attackerDamageTaken = attackerDamage;
            }

            return result;
        }

        private static HitAngle CalculateHitAngle(HexCoordinates attCoords, HexCoordinates defCoords, int defFacing)
        {
            // Basit yön açısı karşılaştırması
            int deltaQ = attCoords.Q - defCoords.Q;
            int deltaR = attCoords.R - defCoords.R;

            for (int i = 0; i < 6; i++)
            {
                HexCoordinates dir = HexCoordinates.Directions[i];
                if (dir.Q == deltaQ && dir.R == deltaR)
                {
                    int angleDiff = Mathf.Abs(i - defFacing);
                    if (angleDiff > 3) angleDiff = 6 - angleDiff;

                    return angleDiff switch
                    {
                        0 => HitAngle.Front,
                        1 => HitAngle.Flank,
                        2 => HitAngle.Flank,
                        3 => HitAngle.Rear,
                        _ => HitAngle.Front
                    };
                }
            }

            return HitAngle.Front;
        }
    }
}
