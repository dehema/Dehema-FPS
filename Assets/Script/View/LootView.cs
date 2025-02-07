using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LootView : BaseView
{
    public override void Init(params object[] _params)
    {
        base.Init(_params);

        floatItemIcon_FloatItemIcon.gameObject.SetActive(false);
        bag_ItemContainer.SetShowFloatItemDelegate(floatItemIcon_FloatItemIcon.OnShowFloatItem);
        bag_ItemContainer.SetHideFloatItemDelegate(floatItemIcon_FloatItemIcon.OnHideFloatItem);
        loot_ItemContainer.SetShowFloatItemDelegate(floatItemIcon_FloatItemIcon.OnShowFloatItem);
        loot_ItemContainer.SetHideFloatItemDelegate(floatItemIcon_FloatItemIcon.OnHideFloatItem);
    }

    public override void OnOpen(params object[] _params)
    {
        base.OnOpen(_params);

        if (loot_ItemContainer.GetAllItems().Count == 0)
        {
            loot_ItemContainer.AddItem("11");
            loot_ItemContainer.AddItem("12");
            loot_ItemContainer.AddItem("13");
            loot_ItemContainer.AddItem("14");
            loot_ItemContainer.AddItem("15");
            loot_ItemContainer.AddItem("16");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InGameUIController.Ins.ToggleView<LootView>();
        }
    }
}
