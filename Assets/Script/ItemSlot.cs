using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ItemSlot : BaseUI
{
    private ItemConfig itemConfig;
    private int count = 1;

    public void setItemConfig(ItemConfig _itemConfig, int _count = 1)
    {
        itemConfig = _itemConfig;
        count = _count;
        ShowIcon();
    }

    public void ShowIcon()
    {
        if (itemConfig != null)
        {
            // 显示图标
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                icon_Image.sprite = Resources.Load<Sprite>("Icon/" + itemConfig.icon);
            }

            // 显示品质颜色
            if (bg != null)
            {
                bg_Image.color = InGameMgr.Ins.GetQualityColor(itemConfig.quality);
            }

            // 如果物品可堆叠且数量大于1，显示数量
            if (countText != null)
            {
                countText.gameObject.SetActive(itemConfig.stackable && count > 1);
                if (count > 1)
                {
                    countText_TextMeshProUGUI.text = count.ToString();
                }
            }
        }
        else
        {
            // 没有物品时隐藏显示
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
            if (bg_Image != null)
            {
                bg_Image.color = Color.white;
            }
            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }
        }
    }
}
