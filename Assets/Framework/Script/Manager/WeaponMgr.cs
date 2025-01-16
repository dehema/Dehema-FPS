using UnityEngine;

public class WeaponMgr : MonoBehaviour
{
    public enum WeaponSwitchState
    {
        Up,                 //准备就绪状态
        Down,               //收起状态
        PutDownPrevious,    //正在收起前一个武器的过渡状态
        PutUpNew,           //正在举起新武器的过渡状态
    }
    [Header("正在瞄准")] public bool IsAiming = false;
    [Header("武器后座最大距离")] public float MaxRecoilDistance;

    const string weaponSpritePath = "Sprite/Weapon/";
    PlayerInput playerInput;
    WeaponCtl activeWeapon;
    WeaponSwitchState m_WeaponSwitchState;
    Vector3 m_AccumulatedRecoil;

    public static WeaponMgr Ins;
    private void Awake()
    {
        Ins = this;
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        if (activeWeapon != null && activeWeapon.IsReloading)
            return;

        if (activeWeapon != null && m_WeaponSwitchState == WeaponSwitchState.Up)
        {
            if (!activeWeapon.AutomaticReload && playerInput.GetReloadButtonDown() && activeWeapon.CurrentAmmoRatio < 1.0f)
            {
                IsAiming = false;
                activeWeapon.StartReloadAnimation();
                return;
            }
            IsAiming = playerInput.GetAimInputHeld();

            bool hasFired = activeWeapon.HandleShootInputs(
                playerInput.GetFireInputDown(),
                playerInput.GetFireInputHeld(),
                playerInput.GetFireInputReleased());

            if (hasFired)
            {
                m_AccumulatedRecoil += Vector3.back * activeWeapon.weaponConfig.recoilForce;
                m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, MaxRecoilDistance);
            }
        }
    }

    public void EquipWeapon(WeaponCtl _weaponCtl)
    {
        activeWeapon = _weaponCtl;
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
