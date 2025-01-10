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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (UIMgr.Ins.IsShow<GameDebugView>())
            {
                UIMgr.Ins.CloseView<GameDebugView>();
            }
            else
            {
                UIMgr.Ins.OpenView<GameDebugView>();
            }
        }
    }

    void InitWeapon()
    {
        EquipWeapon(ConfigMgr.Ins.gunConfig.guns.First().Value);
    }

    void EquipWeapon(GunItemConfig _gunConfig)
    {
        weaponInstance = Instantiate(WeaponMgr.Ins.GetWeaponPrefab(_gunConfig.weaponName), transWeaponParentSocket).GetComponent<WeaponController>();
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
        weaponInstance.Owner = gameObject;
        weaponInstance.SourcePrefab = weaponInstance.gameObject;
        weaponInstance.ShowWeapon(false);
    }
}
