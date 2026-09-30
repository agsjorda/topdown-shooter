[System.Serializable]
public class WeaponSlot
{
    public int slotIndex;
    public Weapon weapon;
    public bool isEquipped;

    // Weapon is a serializable class, so Unity replaces a null reference with a blank
    // instance whenever this slot is serialized (Inspector, play-mode reload). A blank
    // Weapon has no data asset, so treat that as empty too.
    public bool IsEmpty => weapon == null || weapon.weaponData == null;

    public void Clear()
    {
        weapon = null;
        isEquipped = false;
    }

    public void AssignWeapon(Weapon newWeapon)
    {
        weapon = newWeapon;
        isEquipped = false;
    }
}