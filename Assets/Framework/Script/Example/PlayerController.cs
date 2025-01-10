using UnityEngine;
using System.Collections.Generic;

namespace Framework.Example
{
    /// <summary>
    /// 玩家控制器示例类
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region 常量定义
        private const float MAX_HEALTH = 100f;
        private const string PLAYER_TAG = "Player";
        #endregion

        #region 序列化字段
        [SerializeField] 
        private float _movementSpeed = 5f;
        
        [SerializeField] 
        private Transform _weaponHolder;
        #endregion

        #region 私有字段
        private float _currentHealth;
        private bool _isJumping;
        private Vector3 _lastPosition;
        private readonly Dictionary<string, int> _inventoryItems = new Dictionary<string, int>();
        #endregion

        #region 公共属性
        public float CurrentHealth
        {
            get => _currentHealth;
            private set => _currentHealth = Mathf.Clamp(value, 0f, MAX_HEALTH);
        }

        public bool IsAlive => _currentHealth > 0;
        #endregion

        #region Unity生命周期
        private void Awake()
        {
            InitializePlayer();
        }

        private void Start()
        {
            RegisterEvents();
        }

        private void Update()
        {
            HandleMovement();
            CheckPlayerStatus();
        }
        #endregion

        #region 公共方法
        public void TakeDamage(float damageAmount)
        {
            if (!IsAlive) return;
            
            CurrentHealth -= damageAmount;
            OnPlayerDamaged();
        }

        public bool AddItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            if (_inventoryItems.ContainsKey(itemId))
            {
                _inventoryItems[itemId] += amount;
            }
            else
            {
                _inventoryItems.Add(itemId, amount);
            }

            return true;
        }
        #endregion

        #region 私有方法
        private void InitializePlayer()
        {
            _currentHealth = MAX_HEALTH;
            _lastPosition = transform.position;
            _isJumping = false;
        }

        private void HandleMovement()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
            transform.Translate(movement * (_movementSpeed * Time.deltaTime));
        }

        private void CheckPlayerStatus()
        {
            if (!IsAlive)
            {
                HandlePlayerDeath();
            }
        }

        private void HandlePlayerDeath()
        {
            // 处理玩家死亡逻辑
            Debug.Log("Player has died!");
            enabled = false;
        }

        private void OnPlayerDamaged()
        {
            // 处理玩家受伤效果
            Debug.Log($"Player took damage! Current health: {_currentHealth}");
        }

        private void RegisterEvents()
        {
            // 注册游戏事件
        }
        #endregion
    }
}
