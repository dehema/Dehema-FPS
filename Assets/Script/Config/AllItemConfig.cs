using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AllItemConfig : ConfigBase
{
    public Dictionary<string, ItemConfig> items = new Dictionary<string, ItemConfig>();

}

/// <summary>
/// 物品配置类
/// </summary>
[Serializable]
public class ItemConfig
{
    /// <summary>
    /// 物品唯一ID
    /// </summary>
    public string id;

    /// <summary>
    /// 物品类型
    /// 1: 电子用品
    /// 2: 医疗道具
    /// 3: 工具材料
    /// 4: 家居用品
    /// 5: 工艺藏品
    /// 6: 资料情报
    /// </summary>
    public int type;

    /// <summary>
    /// 品质等级
    /// 1: 普通 (白色 #FFFFFF)
    /// 2: 优秀 (绿色 #00FF00)
    /// 3: 稀有 (蓝色 #0000FF)
    /// 4: 史诗 (紫色 #800080)
    /// 5: 传说 (橙色 #FF8000)
    /// </summary>
    public int quality;

    /// <summary>
    /// 物品价值
    /// </summary>
    public int value;

    /// <summary>
    /// 多语言ID
    /// </summary>
    public string langID;

    /// <summary>
    /// 在物品箱中的权重
    /// 权重越高，在物品箱中出现的概率越大
    /// </summary>
    public int weight;

    /// <summary>
    /// 宽
    /// </summary>
    public int sizeX;

    /// <summary>
    /// 高
    /// </summary>
    public int sizeY;

    /// <summary>
    /// 图标
    /// </summary>
    public string icon;

    /// <summary>
    /// 是否可以堆叠
    /// </summary>
    public bool stackable;

    /// <summary>
    /// 堆叠上限
    /// </summary>
    public int maxStack;

    public Vector2Int size => new Vector2Int(sizeX, sizeX);    // 物品尺寸（宽x高）

    /// <summary>
    /// 检查配置是否为空
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(id);
    }
}
