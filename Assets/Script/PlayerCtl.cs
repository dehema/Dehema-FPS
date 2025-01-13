using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Audio;


[RequireComponent(typeof(PlayerInput), typeof(WeaponMgr), typeof(CharacterController))]
public class PlayerCtl : MonoBehaviour
{
    [Tooltip("跳跃音频")] public AudioClip jumpSfx;
    [Tooltip("走路音频")] public AudioClip landSfx;
    PlayerWeaponsManager weaponsManager;
    AudioSource audioSource;
    Transform transFirstPersonSocket;
    Transform transWeaponParentSocket;
    WeaponCtl currWeaponCtl;                        //当前装备的武器
    PlayerInput playerInput;                        //按键输入
    //public bool isGrounded { get; private set; }    //是否在地面上
    public bool isGrounded = true;                  //是否在地面上
    public float maxSpeedOnGround = 10f;            //移动速度
    public float runSpeedModifier = 2f;             //冲刺速度系数
    public Vector3 characterVelocity { get; set; }  //角色速度
    public float MovementSharpnessOnGround = 15;    //移动清晰度
    CharacterController characterController;        //角色控制器
    public float jumpForce = 9f;                    //跳跃力
    float targetCharacterHeight;                    //角色高度
    float capsuleHeightCrouching = 0.9f;            //蹲着时的高度
    public float capsuleHeightStanding = 1.8f;      //站立时的高度
    public float lastTimeJumped = 0;                //上次跳跃时间

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        weaponsManager = GetComponent<PlayerWeaponsManager>();
        transFirstPersonSocket = transform.Find("Main Camera/FirstPersonSocket");
        transWeaponParentSocket = transFirstPersonSocket.transform.Find("WeaponParentSocket");
        InitWeapon();
    }

    void Update()
    {
        Vector3 worldspaceMoveInput = transform.TransformVector(playerInput.GetMoveInput());
        bool isRuning = false;
        float speedModifier = isRuning ? runSpeedModifier : 1f;
        if (isGrounded)
        {
            Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);

            if (isGrounded && playerInput.GetJumpInputDown())
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

    //是否被阻塞
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        //蹲伏
        if (crouched)
        {
            targetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            //有障碍物
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(capsuleHeightStanding), characterController.radius, -1, QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != characterController)
                    {
                        return false;
                    }
                }
            }

            targetCharacterHeight = capsuleHeightStanding;
        }
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
