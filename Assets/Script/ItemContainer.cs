using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
/// <summary>
/// 物品容器系统
/// 基于网格的物品存放系统，支持物品旋转和嵌套存储
/// </summary>
public partial class ItemContainer : BaseUI
{
    [Header("容器设置")] public Vector2Int gridSize = new Vector2Int(8, 8);
    [Header("是否允许物品旋转")] public bool allowRotation = true;
    [Header("是否允许物品嵌套")] public bool allowNesting = false;

    [Header("格子预制体")] public ItemSlot slotPrefab;

    private List<ItemSlotData> items = new List<ItemSlotData>();  // 容器中的所有物品
    private bool[,] occupiedCells;                        // 记录每个格子是否被占用
    private ItemSlot[,] slots;                           // 所有格子的引用
    private GridLayoutGroup gridLayout;                   // 网格布局组件

    void Awake()
    {
        occupiedCells = new bool[gridSize.x, gridSize.y];
        slots = new ItemSlot[gridSize.x, gridSize.y];
        gridLayout = GetComponent<GridLayoutGroup>();
        InitializeGrid();
        UpdateContainerSize();
    }

    private void OnEnable()
    {
        RefreshDisplay();
    }

    /// <summary>
    /// 初始化网格
    /// </summary>
    private void InitializeGrid()
    {
        // 设置网格布局参数
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize.x;

        // 生成所有格子
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                ItemSlot slot = Instantiate(slotPrefab, transform);
                slots[x, y] = slot;
                // 可以设置格子的位置信息或其他属性
                slot.name = $"Slot_{x}_{y}";
            }
        }
    }

    /// <summary>
    /// 获取指定位置的格子
    /// </summary>
    public ItemSlot GetSlotAt(Vector2Int position)
    {
        if (position.x >= 0 && position.x < gridSize.x &&
            position.y >= 0 && position.y < gridSize.y)
        {
            return slots[position.x, position.y];
        }
        return null;
    }

    /// <summary>
    /// 检查指定位置是否可以放置物品
    /// </summary>
    public bool CanPlaceItem(Vector2Int position, Vector2Int size, bool rotated = false)
    {
        Vector2Int actualSize = rotated ? new Vector2Int(size.y, size.x) : size;

        // 检查是否超出边界
        if (position.x < 0 || position.y < 0 ||
            position.x + actualSize.x > gridSize.x ||
            position.y + actualSize.y > gridSize.y)
            return false;

        // 检查是否与其他物品重叠
        for (int x = 0; x < actualSize.x; x++)
        {
            for (int y = 0; y < actualSize.y; y++)
            {
                if (occupiedCells[position.x + x, position.y + y])
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 尝试在指定位置放置物品
    /// </summary>
    public bool TryPlaceItem(ItemConfig item, Vector2Int position, Vector2Int size, bool rotated = false, int count = 1)
    {
        if (!CanPlaceItem(position, size, rotated))
            return false;

        // 创建新的物品槽
        ItemSlotData slot = new ItemSlotData(item, position, rotated, count);

        // 标记占用的格子
        foreach (var cell in slot.GetOccupiedCells())
        {
            occupiedCells[cell.x, cell.y] = true;
        }

        items.Add(slot);

        // 更新物品显示
        Vector2Int topLeft = slot.GetOccupiedCells()[0]; // 获取左上角格子
        if (slots[topLeft.x, topLeft.y] != null)
        {
            slots[topLeft.x, topLeft.y].setItemConfig(item, count);
        }

        return true;
    }

    /// <summary>
    /// 移除指定位置的物品
    /// </summary>
    public ItemSlotData RemoveItemAt(Vector2Int position)
    {
        ItemSlotData slot = items.Find(item => item.GetOccupiedCells().Contains(position));
        if (slot != null)
        {
            // 清除占用标记
            foreach (var cell in slot.GetOccupiedCells())
            {
                occupiedCells[cell.x, cell.y] = false;
            }
            items.Remove(slot);

            // 清除物品显示
            Vector2Int topLeft = slot.GetOccupiedCells()[0]; // 获取左上角格子
            if (slots[topLeft.x, topLeft.y] != null)
            {
                slots[topLeft.x, topLeft.y].setItemConfig(null);
            }
        }
        return slot;
    }

    /// <summary>
    /// 获取指定位置的物品
    /// </summary>
    public ItemSlotData GetItemAt(Vector2Int position)
    {
        return items.Find(item => item.GetOccupiedCells().Contains(position));
    }

    /// <summary>
    /// 获取所有物品
    /// </summary>
    public List<ItemSlotData> GetAllItems()
    {
        return new List<ItemSlotData>(items);
    }

    /// <summary>
    /// 清空容器
    /// </summary>
    public void ClearContainer()
    {
        items.Clear();
        occupiedCells = new bool[gridSize.x, gridSize.y];

        // 清除所有格子的显示
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (slots[x, y] != null)
                {
                    slots[x, y].setItemConfig(null);
                }
            }
        }
    }

    /// <summary>
    /// 根据格子尺寸和数量调整容器尺寸
    /// </summary>
    public void UpdateContainerSize()
    {
        if (gridLayout == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 计算总宽度和高度
        float totalWidth = gridSize.x * gridLayout.cellSize.x + (gridSize.x - 1) * gridLayout.spacing.x;
        float totalHeight = gridSize.y * gridLayout.cellSize.y + (gridSize.y - 1) * gridLayout.spacing.y;

        // 设置RectTransform的尺寸
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    /// <summary>
    /// 添加物品到容器
    /// </summary>
    /// <param name="item">物品配置</param>
    /// <param name="count">数量</param>
    /// <returns>是否添加成功</returns>
    public bool AddItem(string itemID, int count = 1)
    {
        ItemConfig item = ConfigMgr.Ins.GetItemConfig(itemID);
        if (item == null || count <= 0) return false;

        // 如果物品可堆叠，先尝试堆叠到现有物品上
        if (item.stackable)
        {
            foreach (ItemSlotData slot in items)
            {
                if (slot.item.id == item.id && slot.count < item.maxStack)
                {
                    int canAdd = Mathf.Min(count, item.maxStack - slot.count);
                    slot.count += canAdd;
                    count -= canAdd;

                    if (count <= 0) return true;
                }
            }
        }

        // 如果还有剩余数量，寻找空位放置
        while (count > 0)
        {
            // 计算单次放置数量
            int placeCount = Mathf.Min(count, item.maxStack);

            // 寻找可放置的位置
            bool placed = false;
            for (int y = 0; y < gridSize.y && !placed; y++)
            {
                for (int x = 0; x < gridSize.x && !placed; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    //先尝试不旋转
                    if (TryPlaceItem(item, pos, item.size, false, placeCount))
                    {
                        placed = true;
                    }
                    // 如果允许旋转且不旋转放不下，尝试旋转放置
                    else if (allowRotation && TryPlaceItem(item, pos, item.size, true, placeCount))
                    {
                        placed = true;
                    }
                }
            }

            // 如果没有找到位置放置物品，返回失败
            if (!placed)
            {
                return count != item.maxStack; // 如果已经放置了一部分，返回部分成功
            }

            count -= placeCount;
        }
        RefreshDisplay();
        return true;
    }

    /// <summary>
    /// 刷新所有物品显示
    /// </summary>
    public void RefreshDisplay()
    {
        // 先清空所有格子的显示
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (slots[x, y] != null)
                {
                    slots[x, y].setItemConfig(null);
                }
            }
        }

        // 显示所有物品
        foreach (var slotData in items)
        {
            List<Vector2Int> occupiedCells = slotData.GetOccupiedCells();
            // 只在物品左上角的格子显示图标
            Vector2Int topLeft = occupiedCells[0];
            if (topLeft.x >= 0 && topLeft.x < gridSize.x &&
                topLeft.y >= 0 && topLeft.y < gridSize.y)
            {
                ItemSlot slot = slots[topLeft.x, topLeft.y];
                if (slot != null)
                {
                    slot.setItemConfig(slotData.item, slotData.count);
                }
            }
        }
    }

    /// <summary>
    /// 显示或隐藏容器
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible)
        {
            RefreshDisplay();
        }
    }
}
