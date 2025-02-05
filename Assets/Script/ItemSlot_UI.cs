using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class ItemSlot : BaseUI
{
    [HideInInspector]
    public GameObject bg;
    [HideInInspector]
    public Image bg_Image;
    [HideInInspector]
    public GameObject icon;
    [HideInInspector]
    public Image icon_Image;
    [HideInInspector]
    public GameObject countText;
    [HideInInspector]
    public TextMeshProUGUI countText_TextMeshProUGUI;

    internal void _LoadUI()    
    {
        bg = transform.Find("$bg#Image").gameObject;
        bg_Image = bg.GetComponent<Image>();
        icon = transform.Find("$icon#Image").gameObject;
        icon_Image = icon.GetComponent<Image>();
        countText = transform.Find("$countText#TextMeshProUGUI").gameObject;
        countText_TextMeshProUGUI = countText.GetComponent<TextMeshProUGUI>();
    }
}