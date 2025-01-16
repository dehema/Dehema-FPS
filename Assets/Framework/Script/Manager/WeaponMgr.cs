using UnityEngine;

public class WeaponMgr : MonoBehaviour
{
    public enum WeaponSwitchState
    {
        Up,                 //׼������״̬
        Down,               //����״̬
        PutDownPrevious,    //��������ǰһ�������Ĺ���״̬
        PutUpNew,           //���ھ����������Ĺ���״̬
    }
    [Header("������׼")] public bool IsAiming = false;
    [Header("��������������")] public float MaxRecoilDistance;

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
