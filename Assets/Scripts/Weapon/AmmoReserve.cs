using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The player's reserve ammo: one shared pool per weapon type, with a cap.
/// Weapons only hold what is in their magazine; reloads draw from here and
/// ammo pickups add here whether or not the matching weapon is carried.
/// </summary>
[Serializable]
public class AmmoReserve
{
    [Serializable]
    public class Pool
    {
        public WeaponType weaponType;
        [Tooltip("Never runs out (e.g. the starter pistol)")]
        public bool infinite;
        [Min(0)] public int amount;
        [Min(0)] public int max = 100;
    }

    [SerializeField] private List<Pool> pools = new List<Pool> {
        new Pool { weaponType = WeaponType.Pistol, infinite = true, amount = 0, max = 0 },
        new Pool { weaponType = WeaponType.Revolver, amount = 24, max = 72 },
        new Pool { weaponType = WeaponType.AutoRifle, amount = 100, max = 400 },
        new Pool { weaponType = WeaponType.Shotgun, amount = 16, max = 64 },
        new Pool { weaponType = WeaponType.Rifle, amount = 30, max = 150 },
    };

    /// <summary>Fired with the weapon type and its new amount whenever a pool changes.</summary>
    public event Action<WeaponType, int> OnAmmoChanged;

    public bool IsInfinite(WeaponType type) => Find(type)?.infinite ?? false;

    public int GetAmount(WeaponType type)
    {
        var pool = Find(type);
        if (pool == null) return 0;
        return pool.infinite ? int.MaxValue : pool.amount;
    }

    public int GetMax(WeaponType type) => Find(type)?.max ?? 0;

    public bool Has(WeaponType type) => GetAmount(type) > 0;

    public bool IsFull(WeaponType type)
    {
        var pool = Find(type);
        return pool == null || pool.infinite || pool.amount >= pool.max;
    }

    /// <summary>Adds up to the cap. Returns how much was actually added.</summary>
    public int Add(WeaponType type, int amount)
    {
        var pool = Find(type);
        if (pool == null || pool.infinite || amount <= 0) return 0;
        int added = Mathf.Min(amount, pool.max - pool.amount);
        if (added <= 0) return 0;
        pool.amount += added;
        OnAmmoChanged?.Invoke(type, pool.amount);
        return added;
    }

    /// <summary>Removes up to the requested amount. Returns how much was taken.</summary>
    public int Take(WeaponType type, int amount)
    {
        var pool = Find(type);
        if (pool == null || amount <= 0) return 0;
        if (pool.infinite) return amount;
        int taken = Mathf.Min(amount, pool.amount);
        if (taken <= 0) return 0;
        pool.amount -= taken;
        OnAmmoChanged?.Invoke(type, pool.amount);
        return taken;
    }

    private Pool Find(WeaponType type)
    {
        for (int i = 0; i < pools.Count; i++) {
            if (pools[i].weaponType == type) return pools[i];
        }
        return null;
    }
}
