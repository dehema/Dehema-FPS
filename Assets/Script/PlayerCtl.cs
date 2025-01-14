using System;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(PlayerInput), typeof(WeaponMgr), typeof(CharacterController))]
public class PlayerCtl : MonoBehaviour
{
    [Header("空中的重力")] public float gravityDownForce = 20f;
    [Header("脚步音频")] public AudioClip FootstepSfx;
    [Header("跳跃音频")] public AudioClip jumpSfx;
    [Header("落地音频")] public AudioClip landSfx;
    [Header("移动速度")] public float maxSpeedOnGround = 10f;
    [Header("冲刺速度系数")] public float runSpeedModifier = 2f;
    [Header("移动清晰度")] public float MovementSharpnessOnGround = 15;
    [Header("跳跃力")] public float jumpForce = 9f;
    [Header("空中加速度")] public float accelerationSpeedInAir = 25f;
    [Header("空中最大速度")] public float maxSpeedInAir = 10f;
    [Header("站立时的高度")] public float capsuleHeightStanding = 1.8f;
    [Header("蹲着时的高度")] public float capsuleHeightCrouching = 0.9f;
    [Header("地面检测距离")] public float groundCheckDistance = 0.05f;
    [Header("检测地面的层数")] public LayerMask groundCheckLayers = -1;
    [Header("奔跑时声音频率")] public float footstepSfxFrequencyWhileSprinting = 1f;
    [Header("移动时声音频率")] public float FootstepSfxFrequency = 1f;
    CharacterController characterController;        //角色控制器
    Camera playerCamera;
    PlayerWeaponsManager weaponsManager;
    AudioSource audioSource;
    Transform transFirstPersonSocket;
    Transform transWeaponParentSocket;
    WeaponCtl currWeaponCtl;                        //当前装备的武器
    PlayerInput playerInput;                        //按键输入
    public bool isGrounded { get; private set; }    //是否在地面上
    public Vector3 characterVelocity { get; set; }  //角色速度
    public float rotationMultiplier { get { if (WeaponMgr.Ins.isAiming) { return 1f; } return 1f; } }//旋转系数
    float targetCharacterHeight;                    //角色高度
    float _cameraVerticalAngle = 0;                 //相机垂直角度
    float lastTimeJumped = 0;                       //上次跳跃时间
    const float k_JumpGroundingPreventionTime = 0.2f;//跳跃后防止立即检测地面的时间
    const float k_GroundCheckDistanceInAir = 0.07f; //空中地面检测距离
    Vector3 _groundNormal;                          //地面法线
    float footstepDistanceCounter;                  //移动距离

    void Start()
    {
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
        InitWeapon();

        characterController.enableOverlapRecovery = true;
        SetCrouchingState(false, true);
    }

    void Update()
    {
        bool wasGrounded = isGrounded;
        GroundCheck();
        CharacterMovement();

        // 处理落地事件
        if (!wasGrounded && isGrounded)
        {
            audioSource.PlayOneShot(landSfx);
        }
    }

    /// <summary>
    /// 检测角色是否接触地面，并处理接地相关的逻辑
    /// </summary>
    private void GroundCheck()
    {
        // 根据角色是否在地面上选择不同的检测距离
        // 在地面上时：使用角色控制器的皮肤宽度加上地面检测距离
        // 在空中时：使用固定的空中检测距离
        float chosengroundCheckDistance =
            isGrounded ? (characterController.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // 保存上一帧的地面状态
        bool wasGrounded = isGrounded;

        // 重置地面检测状态
        isGrounded = false;

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
                // 2. 地面斜率在可行走范围内
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(_groundNormal))
                {
                    // 确认角色已着地
                    isGrounded = true;

                    // 如果检测到的距离大于角色控制器的皮肤宽度
                    // 则将角色向下移动这段距离，确保完全贴合地面
                    if (hit.distance > characterController.skinWidth)
                    {
                        characterController.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }

        // 如果这一帧刚刚落地，播放落地音效
        if (!wasGrounded && isGrounded)
        {
            audioSource.PlayOneShot(landSfx);
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

        Vector3 worldspaceMoveInput = transform.TransformVector(playerInput.GetMoveInput());
        bool isRuning = false;
        float speedModifier = isRuning ? runSpeedModifier : 1f;
        if (isGrounded)
        {
            Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);

            if (playerInput.GetJumpInputDown())
            {
                if (SetCrouchingState(false, false))
                {
                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                    characterVelocity += Vector3.up * jumpForce;
                    audioSource.PlayOneShot(jumpSfx);
                    lastTimeJumped = Time.time;
                    isGrounded = false;
                }
            }

            //脚步的声音
            float chosenFootstepSfxFrequency = (isRuning ? footstepSfxFrequencyWhileSprinting : FootstepSfxFrequency);
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
        characterController.Move(characterVelocity * Time.deltaTime);
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

        //// Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
        //weaponCtl.Owner = gameObject;
        //weaponCtl.SourcePrefab = weaponCtl.gameObject;
        //weaponCtl.ShowWeapon(true);
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
            targetCharacterHeight = capsuleHeightCrouching;
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
            targetCharacterHeight = capsuleHeightStanding;
        }

        // 更新接地状态，蹲下时为true，站立时为false
        isGrounded = crouched;
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
}
