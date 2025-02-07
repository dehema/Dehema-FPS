using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class LootView : BaseView
{
    [HideInInspector]
    public GameObject bg;
    [HideInInspector]
    public Image bg_Image;
    [HideInInspector]
    public Button bg_Button;
    [HideInInspector]
    public GameObject content;
    [HideInInspector]
    public RectTransform content_Rect;
    [HideInInspector]
    public GameObject bag;
    [HideInInspector]
    public ItemContainer bag_ItemContainer;
    [HideInInspector]
    public GameObject loot;
    [HideInInspector]
    public ItemContainer loot_ItemContainer;
    [HideInInspector]
    public GameObject floatItemIcon;
    [HideInInspector]
    public FloatItemIcon floatItemIcon_FloatItemIcon;

    internal override void _LoadUI()    
    {
        base._LoadUI();
        bg = transform.Find("$bg#Image,Button").gameObject;
        bg_Image = bg.GetComponent<Image>();
        bg_Button = bg.GetComponent<Button>();
        content = transform.Find("$content#Rect").gameObject;
        content_Rect = content.GetComponent<RectTransform>();
        bag = transform.Find("$content#Rect/bg/bagContainerScroll/Viewport/Content/$bag#ItemContainer").gameObject;
        bag_ItemContainer = bag.GetComponent<ItemContainer>();
        loot = transform.Find("$content#Rect/bg/lootContainerScroll/Viewport/Content/$loot#ItemContainer").gameObject;
        loot_ItemContainer = loot.GetComponent<ItemContainer>();
        floatItemIcon = transform.Find("$content#Rect/bg/$floatItemIcon#FloatItemIcon").gameObject;
        floatItemIcon_FloatItemIcon = floatItemIcon.GetComponent<FloatItemIcon>();
    }
}