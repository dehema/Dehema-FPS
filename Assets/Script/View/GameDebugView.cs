using Framework.Example;
using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

public partial class GameDebugView : BaseView
{
    PlayerCtl playerController;
    List<Toggle> tgWeaponList = new List<Toggle>();

    public override void Init(params object[] _params)
    {
        base.Init(_params);
        playerController = FindFirstObjectByType<PlayerCtl>();

        initGuns();
    }

    void initGuns()
    {
        gunItem.SetActive(false);
        foreach (var _config in ConfigMgr.Ins.weaponConfig.guns)
        {
            GameObject item = Instantiate(gunItem, gunItem.transform.parent);
            item.SetActive(true);
            Toggle tg = item.GetComponent<Toggle>();
            GunItemConfig itemConfig = _config.Value;
            tg.targetGraphic.GetComponent<Image>().sprite = WeaponMgr.Ins.GetWeaponIcon(itemConfig.weaponName);
            tgWeaponList.Add(tg);
            tg.onValueChanged.AddListener((bool _isOn) =>
            {
                if (_isOn)
                {
                    playerController.EquipWeapon(itemConfig);
                }
            });
        }
    }
}
