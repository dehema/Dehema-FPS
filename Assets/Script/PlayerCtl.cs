using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Audio;


[RequireComponent(typeof(PlayerInput), typeof(WeaponMgr), typeof(CharacterController))]
public class PlayerCtl : MonoBehaviour
{
    [Tooltip("��Ծ��Ƶ")] public AudioClip jumpSfx;
    [Tooltip("��·��Ƶ")] public AudioClip landSfx;
    PlayerWeaponsManager weaponsManager;
    AudioSource audioSource;
    Transform transFirstPersonSocket;
    Transform transWeaponParentSocket;
    WeaponCtl currWeaponCtl;                        //��ǰװ��������
    PlayerInput playerInput;                        //��������
    //public bool isGrounded { get; private set; }    //�Ƿ��ڵ�����
    public bool isGrounded = true;                  //�Ƿ��ڵ�����
    public float maxSpeedOnGround = 10f;            //�ƶ��ٶ�
    public float runSpeedModifier = 2f;             //����ٶ�ϵ��
    public Vector3 characterVelocity { get; set; }  //��ɫ�ٶ�
    public float MovementSharpnessOnGround = 15;    //�ƶ�������
    CharacterController characterController;        //��ɫ������
    public float jumpForce = 9f;                    //��Ծ��
    float targetCharacterHeight;                    //��ɫ�߶�
    float capsuleHeightCrouching = 0.9f;            //����ʱ�ĸ߶�
    public float capsuleHeightStanding = 1.8f;      //վ��ʱ�ĸ߶�
    public float lastTimeJumped = 0;                //�ϴ���Ծʱ��

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

    //�Ƿ�����
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        //�׷�
        if (crouched)
        {
            targetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            //���ϰ���
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

    //��ȡ��ɫ���������ҵ��°�������ĵ�
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * characterController.radius);
    }

    //��ȡ��ɫ���������ҵ��ϰ�������ĵ�
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - characterController.radius));
    }
}
