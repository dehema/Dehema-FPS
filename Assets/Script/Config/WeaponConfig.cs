using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponConfig : ConfigBase
{
    public Dictionary<string, GunItemConfig> guns = new Dictionary<string, GunItemConfig>();
}

public enum WeaponShootType
{
    Single, //单发
    Semi,   //半自动
    Auto,   //全自动
    Burst,  //三连发
    Charge, //充能
}

public enum WeaponType
{
    Gun,    //枪械
    Melee,  //近战武器
}

//枪械配置
public class GunItemConfig : WeaponItemConfig
{
    public string weaponName;       //武器名称								
    public string bulletPrefab;     //子弹模型
    public string shootingMode;     //射击模式
    public float delayBetweenShots; //射击间隔
    public float bulletSpreadAngle; //子弹散布   
    public int bulletsPerShot;      //弹丸数量/每发
    public float damagePerBullet;   //子弹伤害/每颗
    public int recoilForce;         //后坐力
    public float aimZoomRatio;      //缩放倍率
    public float ammoReloadRate;    //每秒装填的弹药数量
    public float ammoReloadDelay;   //装弹延迟
    public int maxAmmo;             //每个弹夹弹药量

    public bool isEmpty()
    {
        return string.IsNullOrEmpty(weaponName);
    }
}

//武器配置
public class WeaponItemConfig
{
    public WeaponType weaponType;   //枪械类型

    public WeaponItemConfig()
    {
        weaponType = WeaponType.Gun;
    }
}