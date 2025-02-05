using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class ItemSlot : BaseUI, IPointerDownHandler
{
    private ItemConfig itemConfig;
    private int count = 1;
    private Canvas canvas;
    private ItemContainer container;
    public static ItemSlot draggingSlot;

    private void Awake()
    {
        _LoadUI();
        canvas = GetComponentInParent<Canvas>();
        container = GetComponentInParent<ItemContainer>();
    }

    private void Update()
    {
        // 如果当前格子是被拖拽的格子
        if (draggingSlot == this && itemConfig != null)
        {
            // 检测鼠标按键状态
            if (Input.GetMouseButton(0)) // 鼠标左键按住
            {
                // 更新浮动物品的位置
                Vector2Int mousePos = GetMouseSlotPosition();
                // 可以在这里添加预览效果
            }
            else // 鼠标左键释放
            {
                Vector2Int targetPos = GetMouseSlotPosition();
                if (targetPos.x != -1 && targetPos.y != -1)
                {
                    // 如果是同一个容器内的交换
                    if (draggingSlot.container == container)
                    {
                        container.SwapItems(draggingSlot.GetGridPosition(), targetPos);
                    }
                    // 如果是不同容器之间的交换
                    else
                    {
                        // 原有的跨容器交换逻辑
                        ItemContainer sourceContainer = draggingSlot.container;
                        Vector2Int sourcePos = draggingSlot.GetGridPosition();

                        if (container.CanAcceptItem(draggingSlot.GetItemConfig(), targetPos))
                        {
                            ItemSlotData sourceItem = sourceContainer.RemoveItemAt(sourcePos);
                            if (itemConfig == null)
                            {
                                container.TryPlaceItem(sourceItem.item, targetPos, sourceItem.item.size, sourceItem.isRotated, sourceItem.count);
                            }
                            else
                            {
                                ItemSlotData targetItem = container.RemoveItemAt(targetPos);
                                if (!container.TryPlaceItem(sourceItem.item, targetPos, sourceItem.item.size, sourceItem.isRotated, sourceItem.count))
                                {
                                    container.TryPlaceItem(targetItem.item, targetPos, targetItem.item.size, targetItem.isRotated, targetItem.count);
                                    sourceContainer.TryPlaceItem(sourceItem.item, sourcePos, sourceItem.item.size, sourceItem.isRotated, sourceItem.count);
                                }
                                else if (!sourceContainer.TryPlaceItem(targetItem.item, sourcePos, targetItem.item.size, targetItem.isRotated, targetItem.count))
                                {
                                    sourceContainer.TryPlaceItem(sourceItem.item, sourcePos, sourceItem.item.size, sourceItem.isRotated, sourceItem.count);
                                    container.TryPlaceItem(targetItem.item, targetPos, targetItem.item.size, targetItem.isRotated, targetItem.count);
                                }
                            }
                        }
                    }
                }

                container.HideFloatItem();
                draggingSlot = null;
            }
        }
    }

    public void setItemConfig(ItemConfig _itemConfig, int _count = 1)
    {
        itemConfig = _itemConfig;
        count = _count;
        ShowIcon();
    }

    public ItemConfig GetItemConfig() => itemConfig;
    public int GetCount() => count;
    public Vector2Int GetGridPosition() => container.GetSlotPosition(this);
    public Vector2Int GetMouseSlotPosition() => container.GetMouseSlotPosition();

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

        draggingSlot = this;

        // 将拖拽的物品移到最上层
        //transform.SetParent(canvas.transform);
        container.ShowFloatItem(itemConfig);
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData) { }

    public void OnDrop(PointerEventData eventData) { }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (itemConfig == null)
            return;
        draggingSlot = this;
        container.ShowFloatItem(itemConfig);
    }
}
