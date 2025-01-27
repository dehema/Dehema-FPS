using System.Collections.Generic;
using UnityEngine;

public class PlayerMgr : MonoSingleton<PlayerMgr>
{
    public List<ItemSlotData> bagItem;
    Vector2Int bagSize = new Vector2Int(8, 8);

    public void AddItem(int _id)
    {

    }
}
