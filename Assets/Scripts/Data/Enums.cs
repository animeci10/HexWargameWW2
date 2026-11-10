namespace HexWargame.Data
{
    public enum NationFaction
    {
        Germany,
        USSR,
        USA,
        GreatBritain,
        Japan,
        Italy,
        France,
        Poland
    }

    public enum UnitCategory
    {
        RifleInfantry,
        AssaultEngineers,
        MountainInfantry,
        Paratroopers,
        MotorizedInfantry,
        MechanizedInfantry,
        LightTank,
        MediumTank,
        HeavyTank,
        TankDestroyer,
        AssaultGun,
        TowedArtillery,
        RocketArtillery,
        SelfPropelledArtillery,
        AntiAirGun,
        SelfPropelledAA,
        ReconArmoredCar,
        FighterAircraft,
        CloseAirSupportCAS,
        SupplyConvoy,
        HeadquartersHQ
    }

    public enum TargetClass
    {
        SoftInfantry,
        LightVehicle,
        MediumArmor,
        HeavyArmor,
        FortificationBunker,
        Aircraft
    }

    public enum MovementType
    {
        Foot,
        MountainFoot,
        Wheeled,
        HalfTrack,
        Tracked,
        HeavyTracked,
        Amphibious,
        Airborne
    }

    public enum WeatherType
    {
        ClearDry,
        MudRasputitsa,
        SnowWinter,
        HeavyRain
    }

    public enum HitAngle
    {
        Front,  // %100 Zırh
        Flank,  // %60 Zırh + %20 İsabet bonusu
        Rear    // %35 Zırh + Kritik motor arızası riski
    }

    public enum AttackResultType
    {
        TargetDestroyed,
        HeavyDamage,
        LightDamage,
        DeflectedBounced,
        SuppressedPinned,
        ForcedRetreat
    }
}
