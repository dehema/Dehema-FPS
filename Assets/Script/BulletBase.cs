using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

public abstract class BulletBase : MonoBehaviour
{
    public GameObject Owner { get; private set; }
    public Vector3 InitialPosition { get; private set; }
    public Vector3 InitialDirection { get; private set; }
    public Vector3 InheritedMuzzleVelocity { get; private set; }

    public UnityAction OnShoot;

    public void Shoot(WeaponCtl _ctl)
    {
        Owner = _ctl.Owner;
        InitialPosition = transform.position;
        InitialDirection = transform.forward;
        InheritedMuzzleVelocity = _ctl.MuzzleWorldVelocity;

        OnShoot?.Invoke();
    }
}
