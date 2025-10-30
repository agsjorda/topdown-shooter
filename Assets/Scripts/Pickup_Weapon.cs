using UnityEngine;

public class Pickup_Weapon : Interactable
{
    private PlayerWeaponController weaponController;
    [SerializeField] private Weapon_Data weaponData;

    [SerializeField] private BackupWeaponModel[] models;


    private void Start()
    {
        UpdateGameObject();
    }

    [ContextMenu("Update Item Model")]
    public void UpdateGameObject()
    {
        gameObject.name = "Pickup_Weapon - " + weaponData.weaponType.ToString();

        UpdateItemModel();
    }

    public void UpdateItemModel()
    {
        foreach (BackupWeaponModel model in models) {
            model.gameObject.SetActive(false);

            if (model.weaponType == weaponData.weaponType) {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterial(model.GetComponent<MeshRenderer>());

            }

        }
    }

    public override void Interaction()
    {
        weaponController.PickupWeapon(weaponData);
        //Destroy(gameObject, 0.1f);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (weaponController == null)
            weaponController = other.GetComponent<PlayerWeaponController>();
    }
}
