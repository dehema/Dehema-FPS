using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

public class FloatItemIcon : MonoBehaviour
{
    Image icon;

    void Start()
    {
        icon = GetComponent<Image>();
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }

    void ShowFloatItem(ItemConfig _itemConfig)
    {
        icon.sprite = Resources.Load<Sprite>("Icon/" + _itemConfig.icon);
        gameObject.SetActive(true);
        ��Ҫ��������������з�װ
    }

    void HideFloatItem()
    {
        gameObject.SetActive(false);
    }
}
