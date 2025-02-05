using System;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.UI.GridLayoutGroup;

[RequireComponent(typeof(PlayerInput), typeof(WeaponMgr), typeof(CharacterController))]
public class PlayerCtl : MonoBehaviour
{
    [Header("空中的重力")] public float gravityDownForce = 20f;
    [Header("脚步音频")] public AudioClip FootstepSfx;
    [Header("跳跃音频")] public AudioClip jumpSfx;
    [Header("落地音频")] public AudioClip landSfx;
    [Header("落地受伤音频")] public AudioClip fallDamageSfx;
    [Header("移动速度")] public float MaxSpeedOnGround = 10f;
    [Header("冲刺速度系数")] public float runSpeedModifier = 2f;
    [Header("移动清晰度")] public float MovementSharpnessOnGround = 15;
    [Header("跳跃力")] public float jumpForce = 9f;
    [Header("空中加速度")] public float accelerationSpeedInAir = 25f;
    [Header("空中最大速度")] public float maxSpeedInAir = 10f;
    [Header("站立时的高度")] public float capsuleHeightStanding = 1.8f;
    [Header("蹲着时的高度")] public float capsuleHeightCrouching = 0.9f;
    [Tooltip("蹲下转换的速度")] public float crouchingSharpness = 10f;
    [Tooltip("蹲着时的移动速度")][Range(0, 1)] public float maxSpeedCrouchedRatio = 0.5f;
    [Header("地面检测距离")] public float groundCheckDistance = 0.05f;
    [Header("检测地面的层数")] public LayerMask groundCheckLayers = -1;
    [Header("移动时声音频率")] public float footstepSfxFrequency = 0.2f;
    [Header("奔跑时声音频率")] public float footstepSfxFrequencyWhileSprinting = 0.3f;
    [Header("掉落受伤最大速度")] public float minSpeedForFallDamage = 10f;
    [Header("掉落受伤最大速度")] public float maxSpeedForFallDamage = 30f;
    [Header("掉落最小伤害")] public float fallDamageAtMinSpeed = 10F;
    [Header("掉落最大伤害")] public float fallDamageAtMaxSpeed = 50f;
    [Header("玩家是否会承受掉落伤害")] public bool RecievesFallDamage;
    [Header("摄像机所在的字符高度的比率")][Range(0, 1)] public float cameraHeightRatio = 0.9f;
    [Header("检测可交互物体的距离")] public float interactionRange = 2f;
    [Header("检测可交互物体的层级")] public LayerMask interactionLayer = -1;

    CharacterController characterController;        //角色控制器
    Camera playerCamera;                            //主相机
    PlayerWeaponsManager weaponsManager;
    AudioSource audioSource;
    Transform transFirstPersonSocket;
    public Transform transWeaponParentSocket;
    WeaponCtl currWeaponCtl;                        //当前装备的武器
    PlayerInput playerInput;                        //按键输入
    public bool IsGrounded { get; private set; }    //是否在地面上
    public bool HasJumpedThisFrame { get; private set; }
    public bool isCrouching { get; private set; }   //是否蹲伏
    public Vector3 characterVelocity { get; set; }  //角色速度
    public float rotationMultiplier { get { if (WeaponMgr.Ins.IsAiming) { return 1f; } return 1f; } }//旋转系数
    float _targetCharacterHeight;                   //角色高度
    float _cameraVerticalAngle = 0;                 //相机垂直角度
    float lastTimeJumped = 0;                       //上次跳跃时间
    const float k_JumpGroundingPreventionTime = 0.2f;//跳跃后防止立即检测地面的时间
    const float k_GroundCheckDistanceInAir = 0.07f; //空中地面检测距离
    Vector3 _groundNormal;                          //地面法线
    float footstepDistanceCounter;                  //移动距离
    Vector3 latestImpactSpeed;                      //最近一次碰撞的冲击速度
    bool IsAiming;                                  //是否瞄准
    Vector3 m_WeaponMainLocalPosition;              //武器位置
    Vector3 m_WeaponBobLocalPosition;               //武器摆动位置

    public static PlayerCtl Ins;

    private void Awake()
    {
        Ins = this;
        playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        weaponsManager = GetComponent<PlayerWeaponsManager>();
        transFirstPersonSocket = transform.Find("Main Camera/FirstPersonSocket");
        transWeaponParentSocket = transFirstPersonSocket.transform.Find("WeaponParentSocket");
        FootstepSfx = Resources.Load<AudioClip>("Audio/Sound/Footstep");
        jumpSfx = Resources.Load<AudioClip>("Audio/Sound/Jump");
        landSfx = Resources.Load<AudioClip>("Audio/Sound/Land");
    }

    void Start()
    {
        InitWeapon();
        characterController.enableOverlapRecovery = true;
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);
    }

    /// <summary>
    /// 每帧更新角色状态
    /// </summary>
    void Update()
    {
        // 重置跳跃状态标记
        HasJumpedThisFrame = false;

        // 保存上一帧的地面状态，用于检测状态变化
        bool wasGrounded = IsGrounded;
        // 进行地面检测
        GroundCheck();

        // 处理落地逻辑
        if (IsGrounded && !wasGrounded)
        {
            // 计算坠落速度（取垂直速度和最后撞击速度中的较小值的负数）
            float fallSpeed = -Mathf.Min(characterVelocity.y, latestImpactSpeed.y);

            // 计算坠落伤害比例
            // fallSpeedRatio为0表示未达到最小伤害速度，1表示达到最大伤害速度
            float fallSpeedRatio = (fallSpeed - minSpeedForFallDamage) / (maxSpeedForFallDamage - minSpeedForFallDamage);

            // 如果开启了坠落伤害且坠落速度超过最小伤害速度
            if (RecievesFallDamage && fallSpeedRatio > 0f)
            {
                // 根据坠落速度比例计算实际伤害值
                float dmgFromFall = Mathf.Lerp(fallDamageAtMinSpeed, fallDamageAtMaxSpeed, fallSpeedRatio);
                // 播放受伤音效
                audioSource.PlayOneShot(fallDamageSfx);
            }
            else
            {
                // 如果没有受伤，播放普通落地音效
                audioSource.PlayOneShot(landSfx);
            }
        }

        UpdateCharacterHeight(false);
        // 更新角色移动
        CharacterMovement();

        // 检测可交互物体
        CheckInteractable();
    }

    /// <summary>
    /// 检测角色是否接触地面，并处理接地相关的逻辑
    /// </summary>
    private void GroundCheck()
    {
        // 根据角色是否在地面上选择不同的检测距离
        // 在地面上时：使用角色控制器的皮肤宽度加上地面检测距离
        // 在空中时：使用固定的空中检测距离
        float chosengroundCheckDistance = IsGrounded ? (characterController.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // 重置地面检测状态
        IsGrounded = false;
        _groundNormal = Vector3.up;

        // 检查是否已经过了防止连跳的时间（k_JumpGroundingPreventionTime）
        // 这个时间用来防止玩家在跳跃后立即被检测为落地
        if (Time.time >= lastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // 使用胶囊体投射检测地面
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(characterController.height),
                characterController.radius, Vector3.down, out RaycastHit hit, chosengroundCheckDistance, groundCheckLayers,
                QueryTriggerInteraction.Ignore))
            {
                // 记录检测到的地面法线
                _groundNormal = hit.normal;

                // 检查两个条件：
                // 1. 地面法线与角色向上方向的点积大于0（确保不是在天花板上）
                // 2. 地面斜度在可行走范围内
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(_groundNormal))
                {
                    // 确认角色已着地
                    IsGrounded = true;

                    // 如果检测到的距离大于角色控制器的皮肤宽度
                    // 则将角色向下移动这段距离，确保完全贴合地面
                    if (hit.distance > characterController.skinWidth)
                    {
                        characterController.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查斜面是否在可行走的坡度限制内
    /// </summary>
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        // Vector3.Angle计算两个向量之间的夹角（返回0-180度）
        // transform.up是角色的向上方向（通常是(0,1,0)）
        // normal是斜面的法线方向
        // characterController.slopeLimit是角色控制器中设置的最大可行走斜率
        // 如果斜面与地面的夹角小于等于最大可行走斜率，则返回true
        return Vector3.Angle(transform.up, normal) <= characterController.slopeLimit;
    }

    // 更新角色胶囊体高度和相机位置
    void UpdateCharacterHeight(bool force)
    {
        // 立即更新高度（强制模式）
        if (force)
        {
            // 直接设置角色控制器的高度为目标高度
            characterController.height = _targetCharacterHeight;
            // 更新胶囊体中心点位置（位于高度的一半处）
            characterController.center = Vector3.up * characterController.height * 0.5f;
            // 根据相机高度比例更新相机位置
            playerCamera.transform.localPosition = Vector3.up * _targetCharacterHeight * cameraHeightRatio;
            //m_Actor.AimPoint.transform.localPosition = characterController.center;
        }
        // 平滑更新高度（非强制模式）
        else if (characterController.height != _targetCharacterHeight)
        {
            // 使用Lerp平滑过渡调整胶囊体大小和相机位置
            characterController.height = Mathf.Lerp(characterController.height, _targetCharacterHeight, crouchingSharpness * Time.deltaTime);
            // 更新胶囊体中心点
            characterController.center = Vector3.up * characterController.height * 0.5f;
            // 平滑过渡更新相机位置
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition,
                Vector3.up * _targetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
            //m_Actor.AimPoint.transform.localPosition = characterController.center;
        }
    }

    void CharacterMovement()
    {
        //X轴旋转
        {
            transform.Rotate(new Vector3(0f, (playerInput.GetLookInputsHorizontal() * 200 * rotationMultiplier), 0f), Space.Self);
        }

        //Y轴旋转
        {
            _cameraVerticalAngle += playerInput.GetLookInputsVertical() * 200 * rotationMultiplier;
            _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, -89f, 89f);
            playerCamera.transform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0, 0);
        }

        // 定义角色是否处于奔跑状态
        bool isRuning = playerInput.GetSprintInputHeld();
        {
            if (isRuning)
            {
                isRuning = SetCrouchingState(false, false);
            }
            // 根据是否奔跑设置速度修正值
            float speedModifier = isRuning ? runSpeedModifier : 1f;
            Vector3 worldspaceMoveInput = transform.TransformVector(playerInput.GetMoveInput());
            // 如果角色在地面上
            if (IsGrounded)
            {
                // 计算目标速度：基于输入方向、基础速度和速度修正值
                Vector3 targetVelocity = worldspaceMoveInput * MaxSpeedOnGround * speedModifier;
                // 如果处于蹲伏状态，降低移动速度
                if (isCrouching)
                    targetVelocity *= maxSpeedCrouchedRatio;
                // 根据斜面重新调整移动方向
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, _groundNormal) * targetVelocity.magnitude;
                // 平滑插值过渡到目标速度
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
                // 处理跳跃输入
                if (IsGrounded && playerInput.GetJumpInputDown())
                {
                    // 尝试从蹲伏状态站起来（如果处于蹲伏状态）
                    if (SetCrouchingState(false, false))
                    {
                        // 重置垂直速度
                        characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                        // 添加向上的跳跃力
                        characterVelocity += Vector3.up * jumpForce;
                        // 播放跳跃音效
                        audioSource.PlayOneShot(jumpSfx);
                        // 记录最后跳跃时间
                        lastTimeJumped = Time.time;
                        HasJumpedThisFrame = true;
                        // 设置为非接地状态
                        IsGrounded = false;
                        _groundNormal = Vector3.up;
                    }
                }

                //脚步的声音
                float chosenFootstepSfxFrequency = (isRuning ? footstepSfxFrequencyWhileSprinting : footstepSfxFrequency);
                if (footstepDistanceCounter >= 1f / chosenFootstepSfxFrequency)
                {
                    footstepDistanceCounter = 0f;
                    audioSource.PlayOneShot(FootstepSfx);
                }

                // 记录移动距离用于脚步声音
                footstepDistanceCounter += characterVelocity.magnitude * Time.deltaTime;
            }
            else
            {
                // 在空中时添加加速度
                characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                // 限制空中速度到最大值，但只限制水平方向
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                // 给速度施加重力
                characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
            }
        }
        // 获取移动前胶囊体底部和顶部的位置
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(characterController.height);
        characterController.Move(characterVelocity * Time.deltaTime);

        // 检测障碍物并相应调整速度
        latestImpactSpeed = Vector3.zero;
        // 使用胶囊体投射检测前方障碍物
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, characterController.radius,
            characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, -1,
            QueryTriggerInteraction.Ignore))
        {
            // 记录碰撞时的速度，用于计算坠落伤害
            latestImpactSpeed = characterVelocity;

            // 根据碰撞面的法线方向调整角色速度（使角色沿着碰撞面滑动）
            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
        }
    }

    public void InitWeapon()
    {
        if (ConfigMgr.Ins.weaponConfig.isLoaded && ConfigMgr.Ins.weaponConfig.guns.Count > 0)
            EquipWeapon(ConfigMgr.Ins.weaponConfig.guns.First().Value);
    }

    public void EquipWeapon(GunItemConfig _gunConfig)
    {
        if (currWeaponCtl != null)
        {
            Destroy(currWeaponCtl.gameObject);
        }
        currWeaponCtl = Instantiate(WeaponMgr.Ins.GetWeaponPrefab(_gunConfig.weaponName), transWeaponParentSocket).GetComponent<WeaponCtl>();
        currWeaponCtl.transform.localPosition = Vector3.zero;
        currWeaponCtl.transform.localRotation = Quaternion.identity;
        currWeaponCtl.setWeaponConfig(_gunConfig);
        currWeaponCtl.Owner = gameObject;

        WeaponMgr.Ins.EquipWeapon(currWeaponCtl);

        //currWeaponCtl.Owner = gameObject;
        //currWeaponCtl.SourcePrefab = weaponCtl.gameObject;
        //currWeaponCtl.ShowWeapon(true);
    }

    /// <summary>
    /// 设置角色的蹲伏状态
    /// </summary>
    /// <param name="crouched">是否蹲下：true表示蹲下，false表示站立</param>
    /// <param name="ignoreObstructions">是否忽略障碍物：true表示强制切换状态，false表示需要检查头顶障碍物</param>
    /// <returns>true表示状态切换成功，false表示因为有障碍物导致切换失败</returns>
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // 如果是蹲下状态，直接将目标高度设置为蹲伏高度
        if (crouched)
        {
            _targetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            // 如果是要站起来，且不忽略障碍物检测
            if (!ignoreObstructions)
            {
                // 使用Physics.OverlapCapsule检测从脚底到站立高度的胶囊体范围内是否有碰撞物
                // GetCapsuleBottomHemisphere()获取胶囊体底部球心位置
                // GetCapsuleTopHemisphere(capsuleHeightStanding)获取站立时胶囊体顶部球心位置
                // characterController.radius是胶囊体半径
                // -1表示检测所有层级，QueryTriggerInteraction.Ignore表示忽略触发器
                Collider[] standingOverlaps = Physics.OverlapCapsule(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(capsuleHeightStanding), characterController.radius, -1, QueryTriggerInteraction.Ignore);

                // 遍历所有检测到的碰撞体
                foreach (Collider c in standingOverlaps)
                {
                    // 如果碰撞体不是角色自身的碰撞器
                    // 说明头顶有障碍物，无法站起，返回false
                    if (c != characterController)
                    {
                        return false;
                    }
                }
            }

            // 检测通过或忽略障碍物检测，将目标高度设置为站立高度
            _targetCharacterHeight = capsuleHeightStanding;
        }

        // 更新蹲伏状态，蹲下时为true，站立时为false
        isCrouching = crouched;
        return true;
    }

    //获取角色控制器胶囊的下半球的中心点
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * characterController.radius);
    }

    //获取角色控制器胶囊的上半球的中心点
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - characterController.radius));
    }

    //获取一个与给定斜率相切的重定向方向
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    /// <summary>
    /// 检测玩家面前的可交互物体
    /// </summary>
    /// <remarks>
    /// 使用射线检测玩家面前的可交互物体：
    /// - 从摄像机发射射线
    /// - 检测指定层级的碰撞体
    /// - 如果检测到LootBox且按下E键，则触发交互
    /// </remarks>
    private void CheckInteractable()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactionRange, interactionLayer))
        {
            // 尝试获取LootBox组件
            LootBox lootBox = hit.collider.GetComponent<LootBox>();
            if (lootBox != null && Input.GetKeyDown(KeyCode.E))
            {
                // 触发箱子的搜索功能
                if (!lootBox.IsLooted)
                {
                    Timer.Ins.SetTimeOut(() =>
                    {
                        if (!UIMgr.Ins.IsShow<LootView>())
                            InGameUIController.Ins.ToggleView<LootView>();
                    }, 0.3f);
                    lootBox.TryOpenBox();
                }
                else
                    InGameUIController.Ins.ToggleView<LootView>();
            }
        }
    }
}
