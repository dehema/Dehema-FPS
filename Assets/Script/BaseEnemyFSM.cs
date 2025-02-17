using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemyFSM : MonoBehaviour
{
    protected Animator animator;
    protected NavMeshAgent agent;
    
    // 基础状态
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        GetHit,
        Die,
        Sleep,
        Defend
    }
    
    // 动画参数
    protected enum AnimParam
    {
        // 布尔参数
        IsWalking,
        IsRunning,
        IsAttacking,
        
        // 触发器参数
        BasicAttack,
        ClawAttack,
        HornAttack,
        Jump,
        Sleep,
        Defend,
        Die,
        GetHit,
        Scream,
        Walk,
        WalkBack,
        WalkLeft,
        WalkRight,
        Run,
        Idle01,
        Idle02
    }
    
    protected EnemyState currentState = EnemyState.Idle;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if (animator == null)
        {
            Debug.LogError($"Animator component missing on {gameObject.name}");
        }
        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent component missing on {gameObject.name}");
        }
    }

    protected virtual void Start()
    {
        InitializeState();
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            UpdateState();
        }
    }

    // 子类必须实现的方法
    protected abstract void InitializeState();
    protected abstract void UpdateState();

    // 动画参数设置的辅助方法
    protected void SetBool(AnimParam param, bool value)
    {
        animator.SetBool(param.ToString(), value);
    }

    protected void SetTrigger(AnimParam param)
    {
        animator.SetTrigger(param.ToString());
    }

    // 状态切换方法
    protected virtual void ChangeState(EnemyState newState)
    {
        OnExitState(currentState);
        currentState = newState;
        OnEnterState(currentState);
    }

    // 状态进入时的处理
    protected virtual void OnEnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                SetBool(AnimParam.IsWalking, false);
                SetBool(AnimParam.IsRunning, false);
                if (agent != null) agent.isStopped = true;
                break;
                
            case EnemyState.Patrol:
                SetBool(AnimParam.IsWalking, true);
                SetBool(AnimParam.IsRunning, false);
                if (agent != null) agent.isStopped = false;
                break;
                
            case EnemyState.Chase:
                SetBool(AnimParam.IsWalking, false);
                SetBool(AnimParam.IsRunning, true);
                if (agent != null) agent.isStopped = false;
                break;
                
            case EnemyState.Attack:
                SetBool(AnimParam.IsWalking, false);
                SetBool(AnimParam.IsRunning, false);
                SetBool(AnimParam.IsAttacking, true);
                if (agent != null) agent.isStopped = true;
                break;
                
            case EnemyState.GetHit:
                SetTrigger(AnimParam.GetHit);
                if (agent != null) agent.isStopped = true;
                break;
                
            case EnemyState.Sleep:
                SetTrigger(AnimParam.Sleep);
                if (agent != null) agent.isStopped = true;
                break;
                
            case EnemyState.Defend:
                SetTrigger(AnimParam.Defend);
                if (agent != null) agent.isStopped = true;
                break;
                
            case EnemyState.Die:
                isDead = true;
                SetTrigger(AnimParam.Die);
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.enabled = false;
                }
                break;
        }
    }

    // 状态退出时的处理
    protected virtual void OnExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Attack:
                SetBool(AnimParam.IsAttacking, false);
                break;
        }
    }

    // 动画触发方法
    protected virtual void TriggerScream()
    {
        SetTrigger(AnimParam.Scream);
    }

    protected virtual void TriggerJump()
    {
        SetTrigger(AnimParam.Jump);
    }

    // 攻击动画触发方法
    protected virtual void TriggerBasicAttack()
    {
        SetTrigger(AnimParam.BasicAttack);
    }

    protected virtual void TriggerClawAttack()
    {
        SetTrigger(AnimParam.ClawAttack);
    }

    protected virtual void TriggerHornAttack()
    {
        SetTrigger(AnimParam.HornAttack);
    }

    // 移动动画触发方法
    protected virtual void SetWalkAnimation(Vector3 moveDirection)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            float angle = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
            
            // 根据移动方向设置对应的走路动画
            if (Mathf.Abs(angle) < 45f)
                SetTrigger(AnimParam.Walk);
            else if (angle >= 45f && angle < 135f)
                SetTrigger(AnimParam.WalkRight);
            else if (angle <= -45f && angle > -135f)
                SetTrigger(AnimParam.WalkLeft);
            else
                SetTrigger(AnimParam.WalkBack);
        }
    }

    // 待机动画触发方法
    protected virtual void TriggerRandomIdle()
    {
        SetTrigger(Random.value > 0.5f ? AnimParam.Idle01 : AnimParam.Idle02);
    }

    // 受伤和死亡方法
    public virtual void TakeDamage(float damage)
    {
        if (!isDead)
        {
            ChangeState(EnemyState.GetHit);
        }
    }

    public virtual void Die()
    {
        if (!isDead)
        {
            ChangeState(EnemyState.Die);
        }
    }
}
