using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon System/Weapon Data")]
public class Weapon_Data : ScriptableObject
{
    public string weaponName;

    [Header("Magazine Details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Header("Regular Shot")]
    public ShootType shootType;
    public int bulletsPerShot = 1;
    public float fireRate;

    [Header("Burst Shot")]
    public bool isBurstAvailable;
    public bool isBurstModeActive;
    public int burstBulletsPerShot;
    public float burstFireRate;
    public float burstFireDelay = .1f;

    [Header("Weapon Spread")]
    public float baseSpread = 1;
    public float maxSpread = 3;

    public float spreadIncreaseRate = 0.15f;

    [Header("Weapon Generics")]
    public WeaponType weaponType;
    [Range(1, 3)]
    public float reloadSpeed = 1;
    [Range(1, 3)]
    public float equipSpeed = 1;
    [Range(2, 12)]
    public float gunDistance = 4;
    [Range(3, 8)]
    public float cameraDistance = 6;

}

