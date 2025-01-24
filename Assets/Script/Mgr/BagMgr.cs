using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����������
/// </summary>
public class BagMgr : Singleton<BagMgr>
{
    /// <summary>
    /// ������Ʒ��
    /// </summary>
    public class BagSlot
    {
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public LootConfig config;

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public int count;

        public BagSlot(LootConfig config, int count = 1)
        {
            this.config = config;
            this.count = count;
        }
    }

    /// <summary>
    /// �����������
    /// </summary>
    private const int MAX_CAPACITY = 20;

    /// <summary>
    /// ������Ʒ�б�
    /// key: lootID
    /// value: ��Ʒ��
    /// </summary>
    private Dictionary<string, BagSlot> bagItems = new Dictionary<string, BagSlot>();

    /// <summary>
    /// �����Ʒ������
    /// </summary>
    /// <param name="config">��Ʒ����</param>
    /// <param name="count">����</param>
    /// <returns>�Ƿ���ӳɹ�</returns>
    public bool AddItem(LootConfig config, int count = 1)
    {
        if (config == null || count <= 0)
            return false;

        // ����Ѿ��������Ʒ�����Զѵ�
        if (bagItems.ContainsKey(config.lootID))
        {
            bagItems[config.lootID].count += count;
            OnBagChanged();
            return true;
        }

        // �����������������ʧ��
        if (bagItems.Count >= MAX_CAPACITY)
            return false;

        // �������Ʒ
        bagItems.Add(config.lootID, new BagSlot(config, count));
        OnBagChanged();
        return true;
    }

    /// <summary>
    /// �ӱ����Ƴ���Ʒ
    /// </summary>
    /// <param name="lootID">��ƷID</param>
    /// <param name="count">����</param>
    /// <returns>�Ƿ��Ƴ��ɹ�</returns>
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
    /// ��ȡ��Ʒ����
    /// </summary>
    /// <param name="lootID">��ƷID</param>
    /// <returns>��Ʒ����</returns>
    public int GetItemCount(string lootID)
    {
        if (string.IsNullOrEmpty(lootID))
            return 0;

        if (!bagItems.ContainsKey(lootID))
            return 0;

        return bagItems[lootID].count;
    }

    /// <summary>
    /// ��ȡ���б�����Ʒ
    /// </summary>
    /// <returns>������Ʒ�б�</returns>
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
    /// ��ձ���
    /// </summary>
    public void Clear()
    {
        bagItems.Clear();
        OnBagChanged();
    }

    /// <summary>
    /// �������ݱ仯ʱ����
    /// </summary>
    private void OnBagChanged()
    {
        // ֪ͨUI����
        // TODO: �����¼���ֱ�Ӹ���UI
    }
}
