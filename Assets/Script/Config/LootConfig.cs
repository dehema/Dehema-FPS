using UnityEngine;

/// <summary>
/// 战利品配置类
/// </summary>
public class LootConfig
{
    /// <summary>
    /// 战利品唯一ID
    /// </summary>
    public string lootID;

    /// <summary>
    /// 战利品类型
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
    /// 在战利品箱中的权重
    /// 权重越高，在战利品箱中出现的概率越大
    /// </summary>
    public int weight;

    public LootConfig()
    {
        lootID = string.Empty;
        type = 1;
        quality = 1;
        value = 0;
        langID = string.Empty;
        weight = 100;
    }

    /// <summary>
    /// 检查配置是否为空
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(lootID);
    }

    /// <summary>
    /// 获取品质颜色
    /// </summary>
    public Color GetQualityColor()
    {
        switch (quality)
        {
            case 1: // 普通 - 白色
                return Color.white;
            case 2: // 优秀 - 绿色
                return Color.green;
            case 3: // 稀有 - 蓝色
                return Color.blue;
            case 4: // 史诗 - 紫色
                return new Color(0.5f, 0f, 0.5f);
            case 5: // 传说 - 橙色
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
}
