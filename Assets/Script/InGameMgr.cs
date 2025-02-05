using System.Collections.Generic;
using UnityEngine;

public class InGameMgr : MonoSingleton<InGameMgr>
{

    public bool isGameEnding;  //游戏是否结束

    /// <summary>
    /// 获取品质颜色
    /// </summary>
    public Color GetQualityColor(int _quality)
    {
        switch (_quality)
        {
            case 1: // 普通 - 白色
                return Color.white;
            case 2: // 优秀 - 绿色
                return new Color(126 / 255f, 224 / 255f, 126 / 255f);
            case 3: // 稀有 - 蓝色
                return new Color(74 / 255f, 156 / 255f, 200 / 255f);
            case 4: // 史诗 - 紫色
                return new Color(0.5f, 0f, 0.5f);
            case 5: // 传说 - 橙色
                return new Color(1f, 0.5f, 0f);
            default:
                return Color.white;
        }
    }
}
