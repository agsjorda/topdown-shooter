using UnityEngine;

public class Enemy_WeaponModel : MonoBehaviour
{
    public Enemy_MeleeWeaponType weaponType;
    public AnimatorOverrideController overrideController;
    public Enemy_MeleeWeaponData weaponData;

    [SerializeField] private GameObject[] trailEffects;

    public void EnableTrailEffects(bool enable)
    {
        foreach (GameObject trail in trailEffects) {
            trail.SetActive(enable);
        }
    }
}
