using UnityEngine;

public class WeaponMgr : Singleton<WeaponMgr>
{
    const string weaponSpritePath = "Sprite/Weapon/";

    //获取武器图标
    public Sprite GetWeaponIcon(string _weaponName)
    {
        return Resources.Load<Sprite>(weaponSpritePath + _weaponName);
    }

    //获取武器预制体
    public GameObject GetWeaponPrefab(string _weaponName)
    {
        return Resources.Load<GameObject>("Prefab/Weapon/Weapon_Shotgun_" + _weaponName);
    }
}
