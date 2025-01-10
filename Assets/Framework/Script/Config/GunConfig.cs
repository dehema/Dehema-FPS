using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GunConfig : ConfigBase
{
    [SerializeField]
    private Dictionary<string, GunItemConfig> guns = new Dictionary<string, GunItemConfig>();

    public bool TryGetGunConfig(string gunId, out GunItemConfig config)
    {
        return guns.TryGetValue(gunId, out config);
    }

    public void AddGunConfig(string gunId, GunItemConfig config)
    {
        if (string.IsNullOrEmpty(gunId))
            throw new ArgumentException("Gun ID cannot be null or empty");
            
        guns[gunId] = config ?? throw new ArgumentNullException(nameof(config));
    }

    public bool RemoveGunConfig(string gunId)
    {
        return guns.Remove(gunId);
    }

    public IReadOnlyDictionary<string, GunItemConfig> GetAllGunConfigs()
    {
        return guns;
    }
}

[Serializable]
public class GunItemConfig
{
    [Tooltip("武器的唯一名称")]
    public string weaponName;       //武器名称

    [Tooltip("子弹预制体的路径")]
    public string projectilePrefab; //子弹模型

    [Tooltip("射击模式：Single(单发), SemiAuto(半自动), FullAuto(全自动), Burst(三连发)")]
    public ShootingMode shootingMode = ShootingMode.Single;     //射击模式（单发，半自动，全自动,三连发）

    [Tooltip("射击间隔(秒)")]
    [Range(0.05f, 5f)]
    public float delayBetweenShots = 0.1f; //射击间隔

    [Tooltip("子弹散布角度")]
    [Range(0f, 45f)]
    public float bulletSpreadAngle;   //子弹散布   

    [Tooltip("每次射击的弹丸数量")]
    [Range(1, 20)]
    public int bulletsPerShot = 1;      //弹丸数量/每发

    [Tooltip("每颗子弹的伤害值")]
    [Range(0f, 1000f)]
    public float damagePerBullet;   //子弹伤害/每颗

    [Tooltip("后坐力强度")]
    [Range(0, 100)]
    public int recoilForce;         //后坐力

    [Tooltip("瞄准时的缩放倍率")]
    [Range(1f, 10f)]
    public float aimZoomRatio = 1f;      //缩放倍率

    [Tooltip("每秒装填的弹药数量")]
    [Range(0.1f, 100f)]
    public float ammoReloadRate = 1f;    //每秒装填的弹药数量

    [Tooltip("开始装弹的延迟时间(秒)")]
    [Range(0f, 5f)]
    public float ammoReloadDelay = 1f;   //装弹延迟

    [Tooltip("弹夹最大容量")]
    [Range(1, 1000)]
    public int maxAmmo = 30;             //每个弹夹弹药量

    // 验证配置是否有效
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(weaponName)
            && !string.IsNullOrEmpty(projectilePrefab)
            && delayBetweenShots > 0
            && bulletsPerShot > 0
            && damagePerBullet >= 0
            && recoilForce >= 0
            && aimZoomRatio >= 1
            && ammoReloadRate > 0
            && ammoReloadDelay >= 0
            && maxAmmo > 0;
    }
}

public enum ShootingMode
{
    Single,     // 单发
    SemiAuto,   // 半自动
    FullAuto,   // 全自动
    Burst       // 三连发
}
