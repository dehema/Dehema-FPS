using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class BagView : BaseView
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
    public GameObject floatItemIcon;
    [HideInInspector]
    public FloatItemIcon floatItemIcon_FloatItemIcon;
    [HideInInspector]
    public GameObject hp;
    [HideInInspector]
    public Slider hp_Slider;

    internal override void _LoadUI()    
    {
        base._LoadUI();
        bg = transform.Find("$bg#Image,Button").gameObject;
        bg_Image = bg.GetComponent<Image>();
        bg_Button = bg.GetComponent<Button>();
        content = transform.Find("$content#Rect").gameObject;
        content_Rect = content.GetComponent<RectTransform>();
        floatItemIcon = transform.Find("$content#Rect/bg/$floatItemIcon#FloatItemIcon").gameObject;
        floatItemIcon_FloatItemIcon = floatItemIcon.GetComponent<FloatItemIcon>();
        hp = transform.Find("$content#Rect/bg/Hp/$hp#Slider").gameObject;
        hp_Slider = hp.GetComponent<Slider>();
    }
}