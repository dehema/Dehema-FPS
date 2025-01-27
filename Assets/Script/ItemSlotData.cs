using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 容器内的物品槽位置信息
/// </summary>
public class ItemSlotData
{
    public ItemConfig item;        // 物品配置
    public Vector2Int position;    // 在网格中的位置
    public Vector2Int size { get { return new Vector2Int(item.sizeX, item.sizeX); } }        // 物品尺寸（宽x高）
    public bool isRotated;         // 是否旋转90度
    public int count;              // 堆叠数量

    /// <summary>
    /// 创建一个物品槽
    /// </summary>
    /// <param name="item">物品配置信息</param>
    /// <param name="position">在网格中的位置</param>
    /// <param name="size">物品尺寸（宽x高）</param>
    /// <param name="isRotated">是否旋转90度</param>
    /// <param name="count">堆叠数量</param>
    public ItemSlotData(ItemConfig item, Vector2Int position, bool isRotated = false, int count = 1)
    {
        this.item = item;
        this.position = position;
        this.isRotated = isRotated;
        this.count = count;
    }

    /// <summary>
    /// 获取物品占用的所有格子坐标
    /// </summary>
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        Vector2Int actualSize = isRotated ? new Vector2Int(size.y, size.x) : size;

        for (int x = 0; x < actualSize.x; x++)
        {
            for (int y = 0; y < actualSize.y; y++)
            {
                cells.Add(new Vector2Int(position.x + x, position.y + y));
            }
        }
        return cells;
    }
}