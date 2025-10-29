using UnityEngine;

public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}

public enum ShootType
{
    Single,
    Auto
}

[System.Serializable] // This attribute makes the class visible in the Unity Inspector

public class Weapon
{
    public WeaponType weaponType;

    [Header("Shooting Info")]
    public ShootType shootType;
    public int bulletsPerShot;
    public float defaultFireRate;
    public float fireRate = 1f; // bullets per second
    private float lastShotTime;

    [Header("Burst Fire")]
    public bool isBurstAvailable;
    public bool isBurstModeActive;

    public int burstBulletsPerShot;
    public float burstFireRate;
    public float burstFireDelay = .1f; // Delay between shots in a burst

    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1, 3)]
    public float reloadSpeed = 1;
    [Range(1, 3)]
    public float equipSpeed = 1;
    [Range(2, 12)]
    public float gunDistance = 4;
    [Range(3, 8)]
    public float cameraDistance = 6;

    [Header("Spread")]
    public float baseSpread = 1;
    private float currentSpread = 2;
    public float maxSpread = 3;

    public float spreadIncreaseRate = 0.15f;

    private float lastSpreadUpdateTime;
    public float spreadResetTime = 1f; // Time in seconds after which the spread resets to base value

    #region Spread Methods
    public Vector3 ApplySpread(Vector3 originalDirection)
    {

        UpdateSpread();

        float randomizedValue = Random.Range(-currentSpread, currentSpread); // Random value between -spreadAmount and +spreadAmount

        //Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue, randomizedValue);
        //Quaternion spreadRotation = Quaternion.AngleAxis(randomizedValue, Vector3.up);

        // Sample a random point inside a circle and use it as yaw/pitch offsets (degrees)
        Vector2 offset = Random.insideUnitCircle * currentSpread; // x = yaw, y = pitch

        // Apply yaw (around up) then pitch (around right)
        Quaternion spreadRotation = Quaternion.AngleAxis(offset.x, Vector3.up) *
                                    Quaternion.AngleAxis(offset.y, Vector3.right);


        return (spreadRotation * originalDirection).normalized;
    }

    private void UpdateSpread()
    {

        if (Time.time > lastSpreadUpdateTime + spreadResetTime)
            currentSpread = baseSpread;
        else
            IncreaseSpread();

        lastSpreadUpdateTime = Time.time;
    }

    private void IncreaseSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread + spreadIncreaseRate, baseSpread, maxSpread);
    }
    #endregion

    #region Burst Mode Methods

    public bool BurstActivated()
    {
        if (weaponType == WeaponType.Shotgun) {
            burstFireDelay = 0;

            return true;
        }
        return isBurstModeActive;
    }

    public void ToggleBurstMode()
    {
        if (isBurstAvailable == false)
            return;

        isBurstModeActive = !isBurstModeActive;

        if (isBurstModeActive) {
            bulletsPerShot = burstBulletsPerShot;
            fireRate = burstFireRate;
        } else {
            bulletsPerShot = 1;
            fireRate = defaultFireRate;
        }
    }
    #endregion

    public bool CanShoot() => HaveEnoughBulltes() && ReadyToFire();

    private bool ReadyToFire()
    {
        if (Time.time > lastShotTime + (1f / fireRate)) {
            lastShotTime = Time.time;
            return true;
        }
        return false;
    }

    #region Reload Methods
    public bool CanReload()
    {
        if (bulletsInMagazine == magazineCapacity)
            return false; // Magazine is already full

        return totalReserveAmmo > 0 ? true : false;
    }
    public void ReloadBullets()
    {
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
