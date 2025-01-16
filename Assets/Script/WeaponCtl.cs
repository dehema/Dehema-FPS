using UnityEngine;

public class WeaponCtl : MonoBehaviour
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }
    public GunItemConfig weaponConfig;         //武器配置
    Transform weaponRoot;               //武器节点
    Transform bulletRoot;               //子弹模型
    GameObject bulletPrefab;            //子弹模型
    GameObject mainWeapon;              //主武器   

    [Header("武器后座速度")] public float RecoilSharpness = 50f;
    [Header("武器后座恢复速度")] public float RecoilRestitutionSharpness = 10f;
    [Header("是否在换弹")] public bool IsReloading; 
    [Header("自动换弹")] public bool AutomaticReload;
    bool _wantsToShoot = false;         //是否要进行射击
    WeaponShootType _weaponShootType;   //射击模式
    float m_CurrentAmmo;                //当前弹药量
    int m_CarriedPhysicalBullets;       //携带的弹药量    
    float lastTimeShot;                 //上次射击时间
    public float CurrentAmmoRatio { get; private set; }
    Vector3 m_WeaponRecoilLocalPosition;
    Vector3 m_AccumulatedRecoil;


    void Awake()
    {
        weaponRoot = transform.Find("WeaponRoot");
        bulletRoot = transform.Find("BulletRoot");
    }

    public void setWeaponConfig(WeaponItemConfig _weaponItemConfig)
    {
        if (_weaponItemConfig.weaponType == WeaponType.Gun)
        {
            weaponConfig = _weaponItemConfig as GunItemConfig;
            bulletPrefab = Resources.Load<GameObject>("Prefab/Bullet/" + weaponConfig.bulletPrefab);
        }
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        _wantsToShoot = inputDown || inputHeld;
        switch (_weaponShootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }
                return false;
            default:
                return false;
        }
    }

    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1f && lastTimeShot + weaponConfig.delayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1f;
            return true;
        }
        return false;
    }

    void HandleShoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bulletItem = Instantiate(bulletPrefab, bulletRoot.position, bulletRoot.rotation, bulletRoot.transform);
            // Unparent the muzzleFlashInstance
            if (bulletItem)
                bulletItem.transform.SetParent(null);
            Destroy(bulletItem, 2f);
        }
        lastTimeShot = Time.time;

    }

    /// <summary>
    /// 枪械后坐力位置
    /// </summary>
    /// <returns></returns>
    public Vector3 UpdateWeaponRecoil()
    {
        // 后坐力距离太远就向前拉近
        if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil, RecoilSharpness * Time.deltaTime);
        }
        // 移动后坐位置，使其恢复到初始位置
        else
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero, RecoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
        }
        return m_WeaponRecoilLocalPosition;
    }

    //开始换弹动作
    public void StartReloadAnimation()
    {
        if (m_CurrentAmmo < weaponConfig.maxAmmo && m_CarriedPhysicalBullets > 0)
        {
            GetComponent<Animator>().SetTrigger("Reload");
            IsReloading = true;
        }
    }
}
