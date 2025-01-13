using Unity.FPS.Game;
using UnityEngine;

public class WeaponCtl : MonoBehaviour
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }
    GunItemConfig weaponConfig;         //武器配置
    Transform weaponRoot;               //武器节点
    Transform bulletRoot;               //子弹模型
    GameObject bulletPrefab;            //子弹模型
    GameObject mainWeapon;              //主武器   
    bool _wantsToShoot = false;         //是否要进行射击
    WeaponShootType _weaponShootType;   //射击模式
    float currentAmmo;                  //当前弹药量
    float lastTimeShot;                 //上次射击时间


    void Awake()
    {
        weaponRoot = transform.Find("WeaponRoot");
        weaponRoot.transform.localPosition = new Vector3(0.17f, -0.05f, 0.05f);
        bulletRoot = transform.Find("BulletRoot");
    }

    void Update()
    {

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
        if (currentAmmo >= 1f && lastTimeShot + weaponConfig.delayBetweenShots < Time.time)
        {
            HandleShoot();
            currentAmmo -= 1f;
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
}
