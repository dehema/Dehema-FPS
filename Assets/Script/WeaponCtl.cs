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
    GunItemConfig weaponConfig;         //��������
    Transform weaponRoot;               //�����ڵ�
    Transform bulletRoot;               //�ӵ�ģ��
    GameObject bulletPrefab;            //�ӵ�ģ��
    GameObject mainWeapon;              //������   
    bool _wantsToShoot = false;         //�Ƿ�Ҫ�������
    WeaponShootType _weaponShootType;   //���ģʽ
    float currentAmmo;                  //��ǰ��ҩ��
    float lastTimeShot;                 //�ϴ����ʱ��


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
