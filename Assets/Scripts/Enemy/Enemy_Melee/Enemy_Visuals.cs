using System.Collections.Generic;
using UnityEngine;


public enum Enemy_MeleeWeaponType { OneHand, Throw, Unarmed }
public class Enemy_Visuals : MonoBehaviour
{
    [Header("Weapon Visuals")]
    [SerializeField] private Enemy_WeaponModel[] weaponModels;
    [SerializeField] private Enemy_MeleeWeaponType currentWeaponType;
    public GameObject currentWeaponModel { get; private set; }

    [Header("Corruption Visuals")]
    [SerializeField] private GameObject[] corruptionCrystals;
    [SerializeField] private int corruptionAmount;

    [Header("Color Textures")]
    [SerializeField] private Texture[] colorTextures;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;


    private void Awake()
    {
        weaponModels = GetComponentsInChildren<Enemy_WeaponModel>(true);
    }
    private void Start()
    {
        // In case Awake wasn't enough (editor changes), refresh
        if (weaponModels == null || weaponModels.Length == 0)
            weaponModels = GetComponentsInChildren<Enemy_WeaponModel>(true);

        CollectCorruptionCrystals();
        corruptionAmount = Random.Range(2, 8);
    }

    public void EnableWeaponTrail(bool enable)
    {
        if (currentWeaponModel != null) {
            Enemy_WeaponModel weaponModelComponent = currentWeaponModel.GetComponent<Enemy_WeaponModel>();
            weaponModelComponent.EnableTrailEffects(enable);
        }
    }


    public void SetupWeaponType(Enemy_MeleeWeaponType weaponType) => currentWeaponType = weaponType;
    public void SetupLook()
    {
        SetupRandomColor();
        SetupRandomWeapon();
        SetupRandomCorruption();
    }

    private void SetupRandomCorruption()
    {
        List<int> availableIndexes = new List<int>();

        for (int i = 0; i < corruptionCrystals.Length; i++) {
            availableIndexes.Add(i);
            corruptionCrystals[i].SetActive(false);
        }

        for (int i = 0; i < corruptionAmount; i++) {

            if (availableIndexes.Count == 0)
                break;

            int randomIndex = Random.Range(0, availableIndexes.Count);
            int objectIndex = availableIndexes[randomIndex];

            corruptionCrystals[objectIndex].SetActive(true);
            availableIndexes.RemoveAt(randomIndex);
        }
    }
    private void SetupRandomWeapon()
    {
        foreach (var weaponModel in weaponModels) {
            weaponModel.gameObject.SetActive(false);
        }

        List<Enemy_WeaponModel> filteredWeaponModels = new List<Enemy_WeaponModel>();

        foreach (var weaponModel in weaponModels) {
            if (weaponModel.weaponType == currentWeaponType) {
                filteredWeaponModels.Add(weaponModel);
            }
        }

        int randomIndex = Random.Range(0, filteredWeaponModels.Count);
        currentWeaponModel = filteredWeaponModels[randomIndex].gameObject;
        currentWeaponModel.SetActive(true);
        OverrideAnimatorControllerIfAble();
    }

    private void OverrideAnimatorControllerIfAble()
    {
        AnimatorOverrideController overrideController = currentWeaponModel.GetComponent<Enemy_WeaponModel>().overrideController;

        if (overrideController != null) {
            GetComponentInChildren<Animator>().runtimeAnimatorController = overrideController;
        }
    }

    private void SetupRandomColor()
    {
        int randomIndex = Random.Range(0, colorTextures.Length);
        Material material = skinnedMeshRenderer.material;
        material.mainTexture = colorTextures[randomIndex];
        skinnedMeshRenderer.material = material;
    }
    private void CollectCorruptionCrystals()
    {
        Enemy_CorruptionCrystal[] crystalComponents = GetComponentsInChildren<Enemy_CorruptionCrystal>(true);

        corruptionCrystals = new GameObject[crystalComponents.Length];
        Debug.Log("Number of corruption crystals: " + corruptionCrystals.Length);

        for (int i = 0; i < crystalComponents.Length; i++) {
            corruptionCrystals[i] = crystalComponents[i].gameObject;
        }
    }
}