using System;
using UnityEngine;
using UnityEngine.Events;

public class WeaponCtl : MonoBehaviour
{
    [Header("武器节点")] Transform weaponRoot;
    [Header("子弹节点")] Transform bulletRoot;
    [Header("射击音频")] AudioClip ShootSfx;
    public GunItemConfig weaponConfig;  //武器配置
    GameObject projectilesPrefab;       //子弹弹道模型
    GameObject bulletPrefab;            //子弹模型
    GameObject bulletFlashPrefab;       //枪口火焰模型
    GameObject mainWeapon;              //主武器   

    [Header("武器后座速度")] public float RecoilSharpness = 50f;
    [Header("武器后座恢复速度")] public float RecoilRestitutionSharpness = 10f;
    [Header("是否在换弹")] public bool IsReloading;
    [Header("自动换弹")] public bool AutomaticReload;
    bool _wantsToShoot = false;         //是否要进行射击
    float m_CurrentAmmo;                //当前弹药量
    int m_CarriedPhysicalBullets;       //携带的弹药量    
    float lastTimeShot;                 //上次射击时间
    public GameObject Owner { get; set; }
    public float CurrentAmmoRatio { get; private set; }         //瞄准倍率
    public Vector3 MuzzleWorldVelocity { get; private set; }    //枪口的世界空间速度，用于计算子弹初始速度
    AudioSource m_ShootAudioSource;        //射击音效的音频源组件
    public UnityAction OnShoot;            //射击事件，在射击时触发
    public event Action OnShootProcessed;  //射击处理完成事件，在射击相关处理完成后触发


    void Awake()
    {
        m_ShootAudioSource = GetComponent<AudioSource>();
        weaponRoot = transform.Find("WeaponRoot");
        bulletRoot = transform.Find("BulletRoot");
    }

    public void setWeaponConfig(WeaponItemConfig _weaponItemConfig)
    {
        if (_weaponItemConfig.weaponType == WeaponType.Gun)
        {
            weaponConfig = _weaponItemConfig as GunItemConfig;
            bulletPrefab = Resources.Load<GameObject>("Prefab/Bullet/" + weaponConfig.bulletPrefab);
            bulletFlashPrefab = Resources.Load<GameObject>("Prefab/BulletFlash/" + weaponConfig.bulletFlashPrefab);
            projectilesPrefab = Resources.Load<GameObject>("Prefab/Projectiles/" + weaponConfig.projectilesPrefab);
            m_CurrentAmmo = weaponConfig.maxAmmo;
            m_CurrentAmmo = 999;
        }
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        _wantsToShoot = inputDown || inputHeld;
        switch ((WeaponShootType)weaponConfig.shootingMode)
        {
            case WeaponShootType.Single:
                if (inputDown)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Auto:
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

    /// <summary>
    /// 处理武器射击的具体逻辑
    /// </summary>
    void HandleShoot()
    {
        // 根据每次射击的子弹数量进行循环
        for (int i = 0; i < weaponConfig.bulletsPerShot; i++)
        {
            // 计算子弹的射击方向（考虑散布度）
            Vector3 shotDirection = GetShotDirectionWithinSpread(bulletRoot);

            // 在枪口位置实例化子弹预制体，并设置其朝向
            GameObject item = Instantiate(projectilesPrefab, bulletRoot.position, Quaternion.LookRotation(shotDirection));

            // 获取子弹的基础组件并初始化射击
            BulletBase newProjectile = item.GetComponent<BulletBase>();
            newProjectile.Shoot(this);
        }

        // 如果有枪口特效预制体，则生成枪口特效
        if (bulletFlashPrefab != null)
        {
            // 在枪口位置创建特效
            GameObject item = Instantiate(bulletFlashPrefab, bulletRoot.position, bulletRoot.rotation, bulletRoot.transform);
            if (item)
                item.transform.SetParent(null); // 将特效从枪口父物体中分离
            Destroy(item, 2f); // 2秒后销毁特效
        }

        // 更新最后射击时间
        lastTimeShot = Time.time;

        // 播放射击音效
        if (ShootSfx)
        {
            m_ShootAudioSource.PlayOneShot(ShootSfx);
        }

        // 触发射击相关的事件
        OnShoot?.Invoke();          // 触发射击事件
        OnShootProcessed?.Invoke(); // 触发射击处理完成事件
    }

    /// <summary>
    /// 计算射击方向，考虑武器的扩散角度
    /// </summary>
    /// <param name="shootTransform">射击的起始Transform，通常是枪口位置</param>
    /// <returns>返回计算后的射击方向向量，已经考虑了扩散角度的随机偏移</returns>
    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        // 将扩散角度转换为0-1之间的比率
        float spreadAngleRatio = weaponConfig.bulletSpreadAngle / 180f;
        // 使用球形插值(Slerp)在forward方向和随机单位球内方向之间进行插值，实现扩散效果
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        return spreadWorldDirection;
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
