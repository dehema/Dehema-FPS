using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunConfig : ConfigBase
{
    public Dictionary<string, GunItemConfig> guns = new Dictionary<string, GunItemConfig>();
}

public class GunItemConfig
{
    public string weaponName;       //武器名称								
    public string projectilePrefab; //子弹模型
    public string shootingMode;     //射击模式（单发，半自动，全自动,三连发）
    public float delayBetweenShots; //射击间隔
    public float bulletSpreadAngle; //子弹散布   
    public int bulletsPerShot;      //弹丸数量/每发
    public float damagePerBullet;   //子弹伤害/每颗
    public int recoilForce;         //后坐力
    public float aimZoomRatio;      //缩放倍率
    public float ammoReloadRate;    //每秒装填的弹药数量
    public float ammoReloadDelay;   //装弹延迟
    public int maxAmmo;             //每个弹夹弹药量
}
