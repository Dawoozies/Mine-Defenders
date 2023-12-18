using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class HeldWeapon : MonoBehaviour, IHeldWeapon
{
    IAgent IHeldWeapon.owner { get => _owner; set => _owner = value; }
    public IAgent _owner;
    IAgent IHeldWeapon.target { get => _target; set => _target = value; }
    public IAgent _target;
    [HideInInspector]
    public HeldWeaponState state;
    public IAnimator[] weaponPartObjectAnimators;
    [HideInInspector]
    public Vector3 _dirToTarget;
    public bool lookAt;
    public bool reverseLookAt;
    Vector3 IHeldWeapon.dirToTarget { set => _dirToTarget = value; }
    public delegate void OnHeldWeaponAttackComplete();
    public event OnHeldWeaponAttackComplete onHeldWeaponAttackComplete;
    public virtual void SetWeaponState(HeldWeaponState state, float stateSpeed)
    {
        //We don't want to be able to move states while attacking
        //We should have a cancel method which can push the weapon out of attacking
        //that is IF we end up needing to cancel for any reason
        if (this.state == state)
            return;
        this.state = state;
        string stateName = state.ToString();
        foreach (IAnimator weaponPartObjectAnimator in weaponPartObjectAnimators)
        {
            weaponPartObjectAnimator.PlayAnimation(stateName, stateSpeed);
        }
    }
    public virtual (HeldWeaponState, bool) StateStatus()
    {
        foreach (IAnimator weaponPartObjectAnimator in weaponPartObjectAnimators)
        {
            if (weaponPartObjectAnimator.GetCurrentAnimationName() != null)
                return (state, false);
        }
        return (state, true);
    }
}
public enum HeldWeaponState
{
    Idle = 0,
    ReadyingAttack = 1,
    Attacking = 2,
    ReturnToIdle = 3,
}
public interface IHeldWeapon
{
    public IAgent owner { get; set; }
    public IAgent target { get; set; }
    public Vector3 dirToTarget { set; }
    public void SetWeaponState(HeldWeaponState state, float stateSpeed);
    public (HeldWeaponState, bool) StateStatus();
}