using UnityEngine;

[System.Serializable]
public class WeaponModel : MonoBehaviour
{
    public WeaponType weaponType;
    public WeaponModelType modelType = WeaponModelType.Primary;
    [Tooltip("Only used for backup models")]
    public HangType hangType;
    public EquipType equipAnimationType;
    public HoldType holdType;

    public Transform gunPoint;
    public Transform holdPoint;

    public void SetActive(bool active) => gameObject.SetActive(active);
    public bool IsHangType(HangType type) => hangType == type;
}