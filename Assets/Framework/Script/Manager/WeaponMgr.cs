using UnityEngine;

public class WeaponMgr : MonoBehaviour
{
    //武器切换状态枚举
    public enum WeaponSwitchState
    {
        Up,                 //准备就绪状态
        Down,               //收起状态
        PutDownPrevious,    //正在收起前一个武器的过渡状态
        PutUpNew,           //正在举起新武器的过渡状态
    }
    [Header("正在瞄准")] public bool IsAiming = false;
    [Header("武器后座最大距离")] public float MaxRecoilDistance = 0.5f;
    [Header("武器相机")] public Camera WeaponCamera;
    [Header("武器后坐力")] public float RecoilSharpness = 50f;
    [Header("武器在后坐力恢复速度")] public float RecoilRestitutionSharpness = 10f;
    [Header("武器摆动频率")] public float BobFrequency = 10f;
    [Header("武器摆动系数")] public float BobSharpness = 10f;
    [Header("武器不瞄准时的摆动")] public float DefaultBobAmount = 0.05f;
    [Header("武器瞄准时的摆动")] public float AimingBobAmount = 0.02f;
    PlayerInput playerInput;                //玩家输入组件
    WeaponCtl activeWeapon;                 //当前激活的武器
    WeaponSwitchState m_WeaponSwitchState;  //武器切换状态
    Vector3 m_AccumulatedRecoil;            //累积的后坐力
    Vector3 m_WeaponRecoilLocalPosition;    //武器后坐力的本地位置
    Vector3 m_WeaponMainLocalPosition;      //武器主要位置
    Vector3 m_WeaponBobLocalPosition;       //武器摆动位置
    Vector3 m_LastCharacterPosition;        //上次人物位置
    float m_WeaponBobFactor;                //武器摆动系数

    //单例实例
    public static WeaponMgr Ins;
    private void Awake()
    {
        Ins = this;
        playerInput = GetComponent<PlayerInput>();
        WeaponCamera = transform.Find("Main Camera/WeaponCamera").GetComponent<Camera>();
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

    void LateUpdate()
    {
        UpdateWeaponBob();
        UpdateWeaponRecoil();

        Vector3 WeaponSocketPos = m_WeaponMainLocalPosition + m_WeaponBobLocalPosition + m_WeaponRecoilLocalPosition;
        PlayerCtl.Ins.transWeaponParentSocket.localPosition = WeaponSocketPos;
    }

    public void EquipWeapon(WeaponCtl _weaponCtl)
    {
        activeWeapon = _weaponCtl;
    }

    //获取武器图标
    public Sprite GetWeaponIcon(string _weaponName)
    {
        return Resources.Load<Sprite>("Icon/Weapon/" + _weaponName);
    }

    //获取武器预制体
    public GameObject GetWeaponPrefab(string _weaponName)
    {
        return Resources.Load<GameObject>("Prefab/Weapon/Weapon_Shotgun_" + _weaponName);
    }

    // 更新武器摆动动画效果
    void UpdateWeaponBob()
    {
        if (Time.deltaTime > 0f)
        {
            // 计算角色的实际移动速度（位置变化/时间）
            Vector3 playerCharacterVelocity = (PlayerCtl.Ins.transform.position - m_LastCharacterPosition) / Time.deltaTime;

            // 计算角色移动因子（用于调整摆动幅度）
            float characterMovementFactor = 0f;
            if (PlayerCtl.Ins.IsGrounded)
            {
                // 将移动速度标准化到0-1之间（相对于最大移动速度）
                characterMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude / (PlayerCtl.Ins.MaxSpeedOnGround * PlayerCtl.Ins.runSpeedModifier));
            }

            // 平滑过渡武器摆动系数
            m_WeaponBobFactor = Mathf.Lerp(m_WeaponBobFactor, characterMovementFactor, BobSharpness * Time.deltaTime);

            // 根据是否瞄准选择摆动幅度
            float bobAmount = IsAiming ? AimingBobAmount : DefaultBobAmount;
            float frequency = BobFrequency;
            // 计算水平摆动值（使用正弦函数）
            float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
            // 计算垂直摆动值（使用正弦函数，频率是水平的2倍，并确保始终为正值）
            float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount * m_WeaponBobFactor;

            // 更新武器的摆动位置
            m_WeaponBobLocalPosition.x = hBobValue;
            m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            // 记录当前位置，用于下一帧计算速度
            m_LastCharacterPosition = PlayerCtl.Ins.transform.position;
        }
    }

    /// <summary>
    /// 更新武器后坐力效果
    /// </summary>
    void UpdateWeaponRecoil()
    {
        // 如果当前后坐力位置已经接近累积后坐力值，则向后坐力目标位置移动
        if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
        {
            // 使用线性插值让武器位置逐渐达到目标后坐力位置，后坐力强度由武器配置决定
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil, RecoilSharpness * Time.deltaTime);
        }
        // 移动后坐位置，使其恢复到初始位置
        else
        {
            // 如果后坐力已经达到最大值，则开始恢复到原始位置
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero, RecoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
        }
    }
}
