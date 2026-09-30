using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AmmoData
{
    public WeaponType weaponType;
    [Range(10, 100)] public int minAmount;
    [Range(10, 100)] public int maxAmount;
}

public class Pickup_Ammo : Interactable
{
    [SerializeField] private AmmoBoxType ammoBoxType;
    [SerializeField] private List<AmmoData> smallBoxAmmo;
    [SerializeField] private List<AmmoData> bigBoxAmmo;
    [SerializeField] private GameObject[] boxModel;

    protected override void Awake()
    {
        SetupBoxModel(); // Setup before base.Awake() so renderer is ready
        base.Awake();
    }

    private void SetupBoxModel()
    {
        // First, deactivate all models
        foreach (var model in boxModel) {
            if (model != null)
                model.SetActive(false);
        }

        // Activate the correct model based on ammoBoxType
        int modelIndex = (int)ammoBoxType;
        if (modelIndex >= 0 && modelIndex < boxModel.Length && boxModel[modelIndex] != null) {
            boxModel[modelIndex].SetActive(true);

            // Get the renderer from the active model
            Renderer activeRenderer = boxModel[modelIndex].GetComponent<Renderer>();
            if (activeRenderer != null) {
                // This updates the base class's objectRenderer reference
                UpdateRenderer(activeRenderer);
            }
        }
    }

    public override void Interaction()
    {
        // Player_Preview carries a disabled PlayerWeaponController; use the live one
        PlayerWeaponController weaponController = null;
        foreach (var candidate in Object.FindObjectsByType<PlayerWeaponController>()) {
            if (candidate.isActiveAndEnabled) {
                weaponController = candidate;
                break;
            }
        }
        if (weaponController == null) {
            Debug.LogWarning($"{name}: No enabled PlayerWeaponController found");
            return;
        }

        List<AmmoData> currentAmmoList = ammoBoxType == AmmoBoxType.bigBox ? bigBoxAmmo : smallBoxAmmo;

        // Ammo goes to the shared reserve even for weapons not carried yet
        int added = 0;
        foreach (var entry in currentAmmoList) {
            added += weaponController.Ammo.Add(entry.weaponType, GetBulletAmount(entry));
        }

        // Every matching pool is full; leave the box for later
        if (added == 0 && currentAmmoList.Count > 0) {
            Debug.Log($"{name}: Ammo reserve full");
            return;
        }

        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
        else
            Destroy(gameObject);
    }

    private int GetBulletAmount(AmmoData ammoData)
    {
        int min = Mathf.Min(ammoData.minAmount, ammoData.maxAmount);
        int max = Mathf.Max(ammoData.minAmount, ammoData.maxAmount);
        return Random.Range(min, max + 1);
    }

    #region Editor
    protected override void OnValidate()
    {
        base.OnValidate();

        // Auto-setup in editor when ammoBoxType changes
        if (boxModel != null && boxModel.Length > 0) {
            // Show correct model in editor
            for (int i = 0; i < boxModel.Length; i++) {
                if (boxModel[i] != null)
                    boxModel[i].SetActive(i == (int)ammoBoxType);
            }
        }
    }
    #endregion
}