using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (UIMgr.Ins.IsShow<GameDebugView>())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                UIMgr.Ins.CloseView<GameDebugView>();
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UIMgr.Ins.OpenView<GameDebugView>();
            }
        }
    }
}
