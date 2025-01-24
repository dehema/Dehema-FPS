using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包管理器
/// </summary>
public class BagMgr : Singleton<BagMgr>
{
    /// <summary>
    /// 背包物品槽
    /// </summary>
    public class BagSlot
    {
        /// <summary>
        /// 物品配置
        /// </summary>
        public LootConfig config;

        /// <summary>
        /// 物品数量
        /// </summary>
        public int count;

        public BagSlot(LootConfig config, int count = 1)
        {
            this.config = config;
            this.count = count;
        }
    }

    /// <summary>
    /// 背包最大容量
    /// </summary>
    private const int MAX_CAPACITY = 20;

    /// <summary>
    /// 背包物品列表
    /// key: lootID
    /// value: 物品槽
    /// </summary>
    private Dictionary<string, BagSlot> bagItems = new Dictionary<string, BagSlot>();

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    /// <param name="config">物品配置</param>
    /// <param name="count">数量</param>
    /// <returns>是否添加成功</returns>
    public bool AddItem(LootConfig config, int count = 1)
    {
        if (config == null || count <= 0)
            return false;

        // 如果已经有这个物品，尝试堆叠
        if (bagItems.ContainsKey(config.lootID))
        {
            bagItems[config.lootID].count += count;
            OnBagChanged();
            return true;
        }

        // 如果背包已满，返回失败
        if (bagItems.Count >= MAX_CAPACITY)
            return false;

        // 添加新物品
        bagItems.Add(config.lootID, new BagSlot(config, count));
        OnBagChanged();
        return true;
    }

    /// <summary>
    /// 从背包移除物品
    /// </summary>
    /// <param name="lootID">物品ID</param>
    /// <param name="count">数量</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveItem(string lootID, int count = 1)
    {
        if (string.IsNullOrEmpty(lootID) || count <= 0)
            return false;

        if (!bagItems.ContainsKey(lootID))
            return false;

        BagSlot slot = bagItems[lootID];
        if (slot.count < count)
            return false;

        slot.count -= count;
        if (slot.count <= 0)
            bagItems.Remove(lootID);

        OnBagChanged();
        return true;
    }

    /// <summary>
    /// 获取物品数量
    /// </summary>
    /// <param name="lootID">物品ID</param>
    /// <returns>物品数量</returns>
    public int GetItemCount(string lootID)
    {
        if (string.IsNullOrEmpty(lootID))
            return 0;

        if (!bagItems.ContainsKey(lootID))
            return 0;

        return bagItems[lootID].count;
    }

    /// <summary>
    /// 获取所有背包物品
    /// </summary>
    /// <returns>背包物品列表</returns>
    public List<BagSlot> GetAllItems()
    {
        List<BagSlot> items = new List<BagSlot>();
        foreach (var slot in bagItems.Values)
        {
            items.Add(slot);
        }
        return items;
    }

    /// <summary>
    /// 清空背包
    /// </summary>
    public void Clear()
    {
        bagItems.Clear();
        OnBagChanged();
    }

    /// <summary>
    /// 背包内容变化时调用
    /// </summary>
    private void OnBagChanged()
    {
        // 通知UI更新
        // TODO: 发送事件或直接更新UI
    }
}
