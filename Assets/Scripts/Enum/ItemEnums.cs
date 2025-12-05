
//Referenced in Item
public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Material,
    Quest,
    Miscellaneous
}


//Referenced in Weapon and WeaponController

public enum ShootType
{
    Single,
    Auto
}

//Referenced in WeaponModel
public enum WeaponType //Referenced in Weapon and WeaponModel
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}
public enum EquipType { SideEquipAnimation, BackEquipAnimation };
public enum HoldType { CommonHold = 1, LowHold, HighHold };

//Pickup_Ammo
public enum AmmoBoxType { smallBox, bigBox }