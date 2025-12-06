[System.Serializable]
public class WeaponSlot
{
    public int slotIndex;
    public Weapon weapon;
    public bool isEquipped;

    public bool IsEmpty => weapon == null;

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