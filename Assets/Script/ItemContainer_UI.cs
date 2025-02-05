using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class ItemContainer : BaseUI
{
    [HideInInspector]
    public GameObject grid;
    [HideInInspector]
    public GridLayoutGroup grid_GridLayoutGroup;

    internal void _LoadUI()    
    {
        grid = transform.Find("$grid#GridLayoutGroup").gameObject;
        grid_GridLayoutGroup = grid.GetComponent<GridLayoutGroup>();
    }
}