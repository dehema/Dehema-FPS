using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameDebugView : BaseView
{
    List<Toggle> tgWeaponList = new List<Toggle>();

    public override void Init(params object[] _params)
    {
        base.Init(_params);
        initGuns();
    }

    void initGuns()
    {
        gunItem.SetActive(false);
        foreach (var _config in ConfigMgr.Ins.gunConfig.guns)
        {
            GameObject item = Instantiate(gunItem, gunItem.transform.parent);
            item.SetActive(true);
            Toggle tg = item.GetComponent<Toggle>();
            tg.targetGraphic.GetComponent<Image>().sprite = WeaponMgr.Ins.GetWeaponIcon(_config.Value.weaponName);
            tgWeaponList.Add(tg);
        }
    }
}
