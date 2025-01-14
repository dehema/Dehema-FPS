using UnityEngine;

public class WeaponMgr : MonoSingleton<WeaponMgr>
{
    const string weaponSpritePath = "Sprite/Weapon/";
    WeaponCtl activeWeapon;
    public bool isAiming = false;

    void Update()
    {

        //bool hasFired = activeWeapon.HandleShootInputs(
        //    m_InputHandler.GetFireInputDown(),
        //    m_InputHandler.GetFireInputHeld(),
        //    m_InputHandler.GetFireInputReleased());

        //// Handle accumulating recoil
        //if (hasFired)
        //{
        //    m_AccumulatedRecoil += Vector3.back * activeWeapon.RecoilForce;
        //    m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
        //}
    }

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
