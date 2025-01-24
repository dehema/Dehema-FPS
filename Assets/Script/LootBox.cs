using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可搜刮战利品箱子的行为控制脚本
/// </summary>
/// <remarks>
/// 该脚本用于处理游戏中可搜刮箱子的核心逻辑，包括：
/// - 战利品的配置和生成
/// - 箱子的交互状态管理
/// - 箱子开启动画控制
/// </remarks>
public class LootBox : MonoBehaviour
{
    /// <summary>
    /// 战利品项配置类，用于在Unity编辑器中配置可能掉落的物品
    /// </summary>
    [System.Serializable]
    public class LootItem
    {
        /// <summary>物品名称</summary>
        public string itemName;
        /// <summary>物品预制体引用</summary>
        public GameObject itemPrefab;
        /// <summary>物品掉落概率（0-100）</summary>
        [Range(0f, 100f)]
        public float dropChance = 100f; // 掉落几率
    }

    [Header("Loot Settings")]
    /// <summary>可能掉落的战利品列表配置</summary>
    [SerializeField] private List<LootItem> possibleLoot; // 可能的战利品列表
    /// <summary>与箱子交互的最大距离</summary>
    [SerializeField] private float interactionDistance = 2f; // 交互距离
    /// <summary>箱子开启动画组件引用</summary>
    [SerializeField] private Animation lootAnimation;   //战利品动画

    /// <summary>标记箱子是否已被搜刮</summary>
    private bool isLooted = false; // 是否已被搜刮
    /// <summary>标记玩家是否在交互范围内</summary>
    private bool playerInRange = false; // 玩家是否在范围内

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        lootAnimation = GetComponent<Animation>();
    }

    /// <summary>
    /// 当玩家进入触发器范围时调用
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowInteractionPrompt(true);
        }
    }

    /// <summary>
    /// 当玩家离开触发器范围时调用
    /// </summary>
    /// <param name="other">离开触发器的碰撞体</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowInteractionPrompt(false);
        }
    }

    /// <summary>
    /// 公共属性供外部检查箱子状态
    /// </summary>
    public bool IsLooted => isLooted;

    /// <summary>
    /// 公共方法供玩家控制器调用
    /// </summary>
    public void TryOpenBox()
    {
        if (isLooted) return;

        foreach (var item in possibleLoot)
        {
            // 根据掉落几率决定是否生成物品
            if (Random.Range(0f, 100f) <= item.dropChance)
            {
                SpawnLoot(item);
            }
        }

        isLooted = true;
        // 可以在这里添加箱子打开的动画或者效果
        ShowInteractionPrompt(false);
    }

    /// <summary>
    /// 生成战利品物品
    /// </summary>
    /// <param name="item">要生成的战利品项</param>
    /// <remarks>
    /// 该方法会在箱子周围随机位置生成指定的战利品
    /// - 使用Random.insideUnitSphere生成随机位置
    /// - 保持物品在箱子同一水平面上（y轴）
    /// - 生成范围限制在箱子0.5单位半径内
    /// </remarks>
    private void SpawnLoot(LootItem item)
    {
        if (item.itemPrefab != null)
        {
            // 在箱子周围随机位置生成物品
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * 0.5f;
            randomPosition.y = transform.position.y; // 确保物品生成在同一平面上
            Instantiate(item.itemPrefab, randomPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// 显示或隐藏交互提示UI，并播放相应动画
    /// </summary>
    /// <param name="show">true显示提示，false隐藏提示</param>
    private void ShowInteractionPrompt(bool show)
    {
        // TODO: 显示或隐藏交互提示UI
        // 这里可以调用UI管理器来显示提示
        lootAnimation.Play();
    }
}
