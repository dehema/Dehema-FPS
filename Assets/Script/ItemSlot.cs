using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class ItemSlot : BaseUI, IPointerDownHandler, IPointerUpHandler
{
    private ItemConfig itemConfig;
    private int count = 1;
    private Canvas canvas;
    private ItemContainer container;
    public static ItemSlot startDraggingSlot;
    public static ItemSlot endDraggingSlot;
    public Vector2Int slotPos;

    public ItemConfig GetItemConfig() => itemConfig;
    public int GetCount() => count;
    public ItemSlot GetMouseItemSlot() => container.GetMouseItemSlot();

    private void Awake()
    {
        _LoadUI();
        canvas = GetComponentInParent<Canvas>();
        container = GetComponentInParent<ItemContainer>();
    }

    private void Update()
    {
        // 如果当前格子是被拖拽的格子
        if (startDraggingSlot == this && itemConfig != null)
        {
            // 检测鼠标按键状态
            if (Input.GetMouseButton(0)) // 鼠标左键按住
            {
                // 更新浮动物品的位置
                // 可以在这里添加预览效果
            }
            else // 鼠标左键释放
            {
                endDraggingSlot = GetMouseItemSlot();
                if (endDraggingSlot != null)
                {
                    // 如果是同一个容器内的交换
                    if (startDraggingSlot.container == endDraggingSlot.container)
                    {
                        container.SwapItems(startDraggingSlot, endDraggingSlot);
                    }
                    // 如果是不同容器之间的交换
                    else
                    {
                        endDraggingSlot.container.SwapItems(startDraggingSlot, endDraggingSlot);
                    }
                }

                container.HideFloatItem();
                startDraggingSlot = null;
                endDraggingSlot = null;
            }
        }
    }

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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemConfig == null) return;

        startDraggingSlot = this;

        // 将拖拽的物品移到最上层
        //transform.SetParent(canvas.transform);
        container.ShowFloatItem(itemConfig);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (itemConfig == null)
            return;
        startDraggingSlot = this;
        container.ShowFloatItem(itemConfig);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // 尝试获取点击位置的ItemSlot组件
            ItemSlot targetSlot = result.gameObject.GetComponent<ItemSlot>();
            if (targetSlot != null)
            {
                endDraggingSlot = targetSlot;
                return;
            }
        }
        endDraggingSlot = null;
    }
}
