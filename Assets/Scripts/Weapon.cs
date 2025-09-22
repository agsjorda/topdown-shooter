using UnityEngine;

public enum WeaponType {
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}

[System.Serializable] // This attribute makes the class visible in the Unity Inspector

public class Weapon {
    public WeaponType weaponType;

    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1, 3)]
    public float reloadSpeed = 1;
    [Range(1, 3)]
    public float equipSpeed = 1;

    [Space]
    public float fireRate = 1f; // bullets per second
    private float lastShotTime;

    public bool CanShoot() {
        if (HaveEnoughBulltes() && ReadyToFire()) {
            bulletsInMagazine--;
            return true;
        }
        return false;
    }

    private bool ReadyToFire() {
        if (Time.time > lastShotTime + (1f / fireRate)) {
            lastShotTime = Time.time;
            return true;
        }
        return false;
    }


    #region Reload Methods
    public bool CanReload() {
        if (bulletsInMagazine == magazineCapacity)
            return false; // Magazine is already full

        return totalReserveAmmo > 0 ? true : false;
    }
    public void ReloadBullets() {
        // Reload drops any remaining bullets in the magazine to simulate real-life reloading
        // totalReserveAmmo += bulletsInMagazine; // Add remaining bullets back to reserve

        int bulletsToReload = magazineCapacity;

        if (bulletsToReload > totalReserveAmmo) {
            bulletsToReload = totalReserveAmmo;
        }

        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine = bulletsToReload;

        if (totalReserveAmmo < 0) {
            totalReserveAmmo = 0;
        }
    }
    private bool HaveEnoughBulltes() => bulletsInMagazine > 0;
    #endregion

}
