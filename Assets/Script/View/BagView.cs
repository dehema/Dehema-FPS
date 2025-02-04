using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BagView : BaseView
{
    [SerializeField] ItemContainer bagContainer;

    public override void Init(params object[] _params)
    {
        base.Init(_params);
        floatItemIcon.SetActive(false);
    }

    public override void OnOpen(params object[] _params)
    {
        base.OnOpen(_params);
        if (bagContainer.GetAllItems().Count == 0)
        {
            bagContainer.AddItem("1");
            bagContainer.AddItem("2");
            bagContainer.AddItem("3");
            bagContainer.AddItem("4");
            bagContainer.AddItem("5");
            bagContainer.AddItem("6");
        }
    }
}
