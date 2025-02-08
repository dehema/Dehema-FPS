using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

public class FloatItemIcon : MonoBehaviour
{
    // 定义委托
    public delegate void ShowFloatItemDelegate(ItemConfig itemConfig);
    public ShowFloatItemDelegate OnShowFloatItem;
    public delegate void HideFloatItemDelegate();
    public HideFloatItemDelegate OnHideFloatItem;

    Image icon;

    void Awake()
    {
        icon = GetComponent<Image>();
        // 初始化委托
        OnShowFloatItem = ShowFloatItem;
        OnHideFloatItem = HideFloatItem;
    }

    private void OnEnable()
    {
        transform.position = Input.mousePosition;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void ShowFloatItem(ItemConfig _itemConfig)
    {
        icon.sprite = Resources.Load<Sprite>("Icon/" + _itemConfig.icon);
        gameObject.SetActive(true);
    }

    public void HideFloatItem()
    {
        gameObject.SetActive(false);
    }
}
