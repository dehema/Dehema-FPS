using Unity.FPS.Game;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Tooltip("灵敏度系数")]
    public float LookSensitivity = 1f;
    [Tooltip("X轴反转")]
    public bool InvertXAxis = false;
    [Tooltip("Y轴反转")]
    public bool InvertYAxis = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked && !InGameMgr.Ins.isGameEnding;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            Vector3 move = new Vector3(Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal), 0f,
                Input.GetAxisRaw(GameConstants.k_AxisNameVertical));
            move = Vector3.ClampMagnitude(move, 1);
            return move;
        }
        return Vector3.zero;
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
        {
            return Input.GetButtonDown(GameConstants.k_ButtonNameJump);
        }
        return false;
    }

    public bool GetSprintInputHeld()
    {
        if (CanProcessInput())
        {
            return Input.GetButton(GameConstants.k_ButtonNameSprint);
        }

        return false;
    }

    public float GetLookInputsHorizontal()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameHorizontal,
            GameConstants.k_AxisNameJoystickLookHorizontal);
    }

    public float GetLookInputsVertical()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameVertical,
            GameConstants.k_AxisNameJoystickLookVertical);
    }

    float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
    {
        if (CanProcessInput())
        {
            // 检查这个输入是否来自鼠标
            bool isGamepad = Input.GetAxis(stickInputName) != 0f;
            float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);

            // 处理垂直输入反转
            if (InvertYAxis)
                i *= -1f;

            // 应用灵敏度倍数
            i *= LookSensitivity;

            if (isGamepad)
            {
                // 由于鼠标输入已经是基于deltaTime的，所以只在输入来自手柄时才需要乘以帧时间
                i *= Time.deltaTime;
            }
            else
            {
                // 减少鼠标输入量使其等效于摇杆移动
                i *= 0.01f;
#if UNITY_WEBGL
                    // 在WebGL中鼠标往往由于鼠标加速度而更加敏感，所以进一步降低它
                    i *= LookSensitivity;
#endif
            }

            return i;
        }

        return 0f;
    }
}
