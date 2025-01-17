using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class ProjectileItem : BulletBase
{
    [Header("基本设置")]
    [Header("子弹碰撞检测的半径")]
    public float Radius = 0.01f;

    [Header("代表子弹根部的变换组件（用于精确的碰撞检测）")]
    public Transform Root;

    [Header("代表子弹尖端的变换组件（用于精确的碰撞检测）")]
    public Transform Tip;

    [Header("子弹的最大存活时间")]
    public float MaxLifeTime = 5f;

    [Header("击中目标时产生的特效预制体")]
    public GameObject ImpactVfx;

    [Header("特效存在的时间")]
    public float ImpactVfxLifetime = 5f;

    [Header("特效生成位置沿击中点法线方向的偏移量")]
    public float ImpactVfxSpawnOffset = 0.1f;

    [Header("击中时播放的音效")]
    public AudioClip ImpactSfxClip;

    [Header("子弹可以碰撞的层")]
    public LayerMask HittableLayers = -1;

    [Header("移动设置")]
    [Header("子弹速度")]
    public float Speed = 20f;

    [Header("重力加速度")]
    public float GravityDownAcceleration = 0f;

    [Header("子弹轨迹修正距离（用于第一人称视图中使子弹向屏幕中心偏移）。小于0时不进行修正")]
    public float TrajectoryCorrectionDistance = -1;

    [Header("决定子弹是否继承武器枪口的速度")]
    public bool InheritWeaponVelocity = false;

    [Header("伤害设置")]
    [Header("子弹伤害值")]
    public float Damage = 40f;

    [Header("伤害区域。如果不需要范围伤害则保持为空")]
    public DamageArea AreaOfDamage;

    [Header("调试设置")]
    [Header("调试视图中子弹半径的颜色")]
    public Color RadiusColor = Color.cyan * 0.2f;

    // 内部变量
    BulletBase m_ProjectileBase;
    Vector3 m_LastRootPosition;    // 上一帧子弹根部的位置
    Vector3 m_Velocity;            // 子弹当前速度
    bool m_HasTrajectoryOverride;  // 是否需要轨迹修正
    float m_ShootTime;            // 发射时间
    Vector3 m_TrajectoryCorrectionVector;        // 轨迹修正向量
    Vector3 m_ConsumedTrajectoryCorrectionVector; // 已使用的轨迹修正向量
    List<Collider> m_IgnoredColliders;          // 需要忽略的碰撞体列表

    // 触发器交互设置
    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    void OnEnable()
    {
        // 获取并检查必要组件
        m_ProjectileBase = GetComponent<BulletBase>();
        DebugUtility.HandleErrorIfNullGetComponent<BulletBase, ProjectileItem>(m_ProjectileBase, this, gameObject);

        m_ProjectileBase.OnShoot += OnShoot;

        // 设置子弹的生命周期
        Destroy(gameObject, MaxLifeTime);
    }

    new void OnShoot()
    {
        // 初始化发射时的基本参数
        m_ShootTime = Time.time;
        m_LastRootPosition = Root.position;
        m_Velocity = transform.forward * Speed;
        m_IgnoredColliders = new List<Collider>();
        transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;

        // 忽略发射者自身的碰撞体
        Collider[] ownerColliders = m_ProjectileBase.Owner.GetComponentsInChildren<Collider>();
        m_IgnoredColliders.AddRange(ownerColliders);

        // 处理玩家射击的特殊情况
        WeaponMgr playerWeaponsManager = m_ProjectileBase.Owner.GetComponent<WeaponMgr>();
        if (playerWeaponsManager)
        {
            // 启用轨迹修正
            m_HasTrajectoryOverride = true;

            // 计算相机到枪口的向量
            Vector3 cameraToMuzzle = (m_ProjectileBase.InitialPosition -
                                      playerWeaponsManager.WeaponCamera.transform.position);

            // 计算轨迹修正向量（投影到相机前方平面上）
            m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                playerWeaponsManager.WeaponCamera.transform.forward);

            // 根据修正距离设置处理轨迹修正
            if (TrajectoryCorrectionDistance == 0)
            {
                transform.position += m_TrajectoryCorrectionVector;
                m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
            }
            else if (TrajectoryCorrectionDistance < 0)
            {
                m_HasTrajectoryOverride = false;
            }

            // 检查子弹初始位置是否已经击中物体
            if (Physics.Raycast(playerWeaponsManager.WeaponCamera.transform.position, cameraToMuzzle.normalized,
                out RaycastHit hit, cameraToMuzzle.magnitude, HittableLayers, k_TriggerInteraction))
            {
                if (IsHitValid(hit))
                {
                    OnHit(hit.point, hit.normal, hit.collider);
                }
            }
        }
    }

    void Update()
    {
        // 更新子弹位置
        transform.position += m_Velocity * Time.deltaTime;
        if (InheritWeaponVelocity)
        {
            transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
        }

        // 处理轨迹修正（使子弹向屏幕中心偏移，即使实际武器位置有偏移）
        if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude <
            m_TrajectoryCorrectionVector.sqrMagnitude)
        {
            Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
            float distanceThisFrame = (Root.position - m_LastRootPosition).magnitude;
            Vector3 correctionThisFrame =
                (distanceThisFrame / TrajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
            m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

            // 检测是否完成修正
            if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                m_HasTrajectoryOverride = false;
            }

            transform.position += correctionThisFrame;
        }

        // 使子弹朝向与速度方向一致
        transform.forward = m_Velocity.normalized;

        // 应用重力
        if (GravityDownAcceleration > 0)
        {
            // 添加重力影响使子弹产生抛物线效果
            m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
        }

        // 碰撞检测
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // 球形射线检测
            Vector3 displacementSinceLastFrame = Tip.position - m_LastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, Radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, HittableLayers,
                k_TriggerInteraction);

            // 找到最近的有效碰撞点
            foreach (var hit in hits)
            {
                if (IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // 处理子弹已经在碰撞体内部的情况
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = Root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }
        }

        m_LastRootPosition = Root.position;
    }

    bool IsHitValid(RaycastHit hit)
    {
        // 忽略带有IgnoreHitDetection组件的碰撞
        if (hit.collider.GetComponent<IgnoreHitDetection>())
        {
            return false;
        }

        // 忽略没有Damageable组件的触发器碰撞
        if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
        {
            return false;
        }

        // 忽略特定的碰撞体（默认为自身碰撞体）
        if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
        {
            return false;
        }

        return true;
    }

    void OnHit(Vector3 point, Vector3 normal, Collider collider)
    {
        // 处理伤害
        if (AreaOfDamage)
        {
            // 范围伤害
            AreaOfDamage.InflictDamageInArea(Damage, point, HittableLayers, k_TriggerInteraction,
                m_ProjectileBase.Owner);
        }
        else
        {
            // 点伤害
            Damageable damageable = collider.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.InflictDamage(Damage, false, m_ProjectileBase.Owner);
            }
        }

        // 生成击中特效
        if (ImpactVfx)
        {
            GameObject impactVfxInstance = Instantiate(ImpactVfx, point + (normal * ImpactVfxSpawnOffset),
                Quaternion.LookRotation(normal));
            if (ImpactVfxLifetime > 0)
            {
                Destroy(impactVfxInstance.gameObject, ImpactVfxLifetime);
            }
        }

        // 播放击中音效
        if (ImpactSfxClip)
        {
            AudioUtility.CreateSFX(ImpactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
        }

        // 销毁子弹
        Destroy(this.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // 在Scene视图中绘制子弹碰撞半径
        Gizmos.color = RadiusColor;
        Gizmos.DrawSphere(transform.position, Radius);
    }
}
