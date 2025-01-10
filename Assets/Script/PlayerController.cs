using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerCharacterController character;
    PlayerWeaponsManager weaponsManager;
    Transform transFirstPersonSocket;
    Transform transWeaponParentSocket;
    WeaponController weaponInstance;    //当前装备的武器

    void Start()
    {
        character = GetComponent<PlayerCharacterController>();
        weaponsManager = GetComponent<PlayerWeaponsManager>();
        transFirstPersonSocket = transform.Find("Main Camera/FirstPersonSocket");
        transWeaponParentSocket = transFirstPersonSocket.transform.Find("WeaponParentSocket");
        InitWeapon();
    }

    void Update()
    {
    }

    public void InitWeapon()
    {
        if (ConfigMgr.Ins.gunConfig.isLoaded && ConfigMgr.Ins.gunConfig.guns.Count > 0)
            EquipWeapon(ConfigMgr.Ins.gunConfig.guns.First().Value);
    }

    public void EquipWeapon(GunItemConfig _gunConfig)
    {
        if (weaponInstance != null)
        {
            Destroy(weaponInstance.gameObject);
        }
        weaponInstance = Instantiate(WeaponMgr.Ins.GetWeaponPrefab(_gunConfig.weaponName), transWeaponParentSocket).GetComponent<WeaponController>();
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
        weaponInstance.Owner = gameObject;
        weaponInstance.SourcePrefab = weaponInstance.gameObject;
        weaponInstance.ShowWeapon(true);
    }
}
