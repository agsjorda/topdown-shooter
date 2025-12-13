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

public enum EquipmentSlotType { Weapon, Headgear, Vest, Boots };

// Armor System Enums
public enum ArmorType
{
    Headgear,  // Helmets, hats, masks
    Vest,      // Body armor, vests, jackets  
    Boots      // Footwear, shoes, greaves
}

//Pickup_Ammo
public enum AmmoBoxType { smallBox, bigBox }

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

public enum WeaponModelType { Primary, Backup }
public enum EquipType { SideEquipAnimation, BackEquipAnimation };
public enum HoldType { CommonHold = 1, LowHold, HighHold };
public enum HangType { LowBackHang, BackHang, SideHang };