using UnityEngine;

public class WeaponMgr : Singleton<WeaponMgr>
{
    const string weaponSpritePath = "Sprite/Weapon/";

    //��ȡ����ͼ��
    public Sprite GetWeaponIcon(string _weaponName)
    {
        return Resources.Load<Sprite>(weaponSpritePath + _weaponName);
    }

    //��ȡ����Ԥ����
    public GameObject GetWeaponPrefab(string _weaponName)
    {
        return Resources.Load<GameObject>("Prefab/Weapon/Weapon_Shotgun_" + _weaponName);
    }
}
