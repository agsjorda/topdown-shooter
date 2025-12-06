[System.Serializable]
public class WeaponSlot
{
    public int slotIndex;
    public Weapon weapon;
    public bool isEquipped;

    public bool IsEmpty => weapon == null;
    public bool HasWeapon => !IsEmpty;

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

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
    }
}