using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public partial class HUDView : BaseView
{
    [SerializeField] private int maxHealth = 100;  // 最大血量

    private static int currentHealth;  // 当前血量
    public int Health
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);  // 确保血量在0到最大值之间
            UpdateHealthDisplay();  // 更新UI显示
        }
    }

    private void Start()
    {
        Health = maxHealth;  // 使用属性设置初始血量
    }

    private void Update()
    {
        UpdateHealthDisplay();
    }

    /// <summary>
    /// 更新血量显示
    /// </summary>
    /// <param name="currentHealth">当前血量值</param>
    private void UpdateHealthDisplay()
    {
        if (health_Image != null)
        {
            health_Image.fillAmount = (float)currentHealth / maxHealth;  // 计算血量百分比
        }

        if (healthValue_TextMeshProUGUI != null)
        {
            healthValue_TextMeshProUGUI.text = currentHealth.ToString();
        }
    }
}
