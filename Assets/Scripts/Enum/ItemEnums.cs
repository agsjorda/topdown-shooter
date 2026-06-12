// Game-specific enums. The old inventory enums (ItemType, EquipmentSlotType, ArmorType)
// were replaced by ScriptableObject assets (ItemCategorySO / EquipmentSlotTypeSO) in the
// InventorySystem module — author them via Create > Inventory System.

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
