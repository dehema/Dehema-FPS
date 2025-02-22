using System.Collections.Generic;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    public static InGameUIController Ins;

    private void Awake()
    {
        Ins = this;
    }
    void Start()
    {
        UIMgr.Ins.OpenView<HUDView>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleView<GameDebugView>();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleView<BagView>();
        }
    }

    /// <summary>
    /// 切换指定界面的显示/隐藏，同时隐藏其他界面
    /// </summary>
    public void ToggleView<T>() where T : BaseView
    {
        bool isViewVisible = UIMgr.Ins.IsShow<T>();

        // 关闭所有界面
        if (UIMgr.Ins.IsShow<GameDebugView>())
            UIMgr.Ins.CloseView<GameDebugView>();
        if (UIMgr.Ins.IsShow<BagView>())
            UIMgr.Ins.CloseView<BagView>();
        if (UIMgr.Ins.IsShow<LootView>())
            UIMgr.Ins.CloseView<LootView>();

        // 如果目标界面原本是隐藏的，就显示它
        if (!isViewVisible)
        {
            UIMgr.Ins.OpenView<T>();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
