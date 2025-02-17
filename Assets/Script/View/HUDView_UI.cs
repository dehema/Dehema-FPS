using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class HUDView : BaseView
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
    public GameObject health;
    [HideInInspector]
    public Image health_Image;
    [HideInInspector]
    public GameObject healthValue;
    [HideInInspector]
    public TextMeshProUGUI healthValue_TextMeshProUGUI;

    internal override void _LoadUI()    
    {
        base._LoadUI();
        bg = transform.Find("$bg#Image,Button").gameObject;
        bg_Image = bg.GetComponent<Image>();
        bg_Button = bg.GetComponent<Button>();
        content = transform.Find("$content#Rect").gameObject;
        content_Rect = content.GetComponent<RectTransform>();
        health = transform.Find("$content#Rect/HorizontalContent/PlayerHealth/$health#Image").gameObject;
        health_Image = health.GetComponent<Image>();
        healthValue = transform.Find("$content#Rect/HorizontalContent/PlayerHealth/$healthValue#TextMeshProUGUI").gameObject;
        healthValue_TextMeshProUGUI = healthValue.GetComponent<TextMeshProUGUI>();
    }
}