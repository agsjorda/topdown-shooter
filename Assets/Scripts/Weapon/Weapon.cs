using UnityEngine;

[System.Serializable] // This attribute makes the class visible in the Unity Inspector

public class Weapon
{
    public WeaponType weaponType;


    #region Regular Shot Variables
    public ShootType shootType;
    public int bulletsPerShot { get; private set; }
    private float defaultFireRate;
    public float fireRate = 1f; // bullets per second
    private float lastShotTime;
    #endregion

    #region Burst Mode Variables
    private bool isBurstAvailable;
    public bool isBurstModeActive;

    private int burstBulletsPerShot;
    private float burstFireRate;
    public float burstFireDelay { get; private set; } // Delay between shots in a burst
    #endregion

    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    // Reserve ammo is shared per weapon type in the player's AmmoReserve, not stored per weapon

    #region Weapon Generic Info

    public float reloadSpeed { get; private set; }
    public float equipSpeed { get; private set; }
    public float gunDistance { get; private set; }
    public float cameraDistance { get; private set; }
    #endregion

    #region Weapon Spread Variables
    [Header("Spread")]
    private float baseSpread = 1;
    private float currentSpread = 2;
    private float maxSpread = 3;

    private float spreadIncreaseRate = 0.15f;

    private float lastSpreadUpdateTime;
    public float spreadResetTime = 1f; // Time in seconds after which the spread resets to base value
    #endregion


    public Weapon_Data weaponData { get; private set; } // serves as default weapon data reference
    public Weapon(Weapon_Data weaponData)
    {
        bulletsInMagazine = weaponData.bulletsInMagazine;
        magazineCapacity = weaponData.magazineCapacity;

        fireRate = weaponData.fireRate;
        weaponType = weaponData.weaponType;

        bulletsPerShot = weaponData.bulletsPerShot;
        shootType = weaponData.shootType;

        isBurstAvailable = weaponData.isBurstAvailable;
        isBurstModeActive = weaponData.isBurstModeActive;
        burstBulletsPerShot = weaponData.burstBulletsPerShot;
        burstFireRate = weaponData.burstFireRate;
        burstFireDelay = weaponData.burstFireDelay;

        baseSpread = weaponData.baseSpread;
        maxSpread = weaponData.maxSpread;

        spreadIncreaseRate = weaponData.spreadIncreaseRate;

        reloadSpeed = weaponData.reloadSpeed;
        equipSpeed = weaponData.equipSpeed;
        gunDistance = weaponData.gunDistance;
        cameraDistance = weaponData.cameraDistance;


        defaultFireRate = fireRate;
        this.weaponData = weaponData;
    }

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
    public int MissingBullets => Mathf.Max(0, magazineCapacity - bulletsInMagazine);

    public bool IsMagazineFull => bulletsInMagazine >= magazineCapacity;

    /// <summary>Tops up the magazine with bullets taken from the reserve.</summary>
    public void LoadBullets(int amount)
    {
        bulletsInMagazine = Mathf.Min(magazineCapacity, bulletsInMagazine + Mathf.Max(0, amount));
    }
    private bool HaveEnoughBulltes() => bulletsInMagazine > 0;
    #endregion

}
