using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldWeapon : MonoBehaviour
{
    HeldWeaponState state;
    IAnimator[] weaponPartObjectAnimators;
    public Vector3 dirToTarget;
    public bool reverseLookAt;
    IProjectile[] projectileInterfaces;
    private void Start()
    {
        weaponPartObjectAnimators = GetComponentsInChildren<IAnimator>();
        projectileInterfaces = GetComponentsInChildren<IProjectile>();
    }
    public void SetWeaponState(HeldWeaponState state, float stateSpeed)
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
        foreach (IProjectile item in projectileInterfaces)
        {
            if (state == HeldWeaponState.ReadyingAttack)
                item.ReadyProjectile();
            if (state == HeldWeaponState.Attacking)
                item.ShootProjectile();
        }
    }
    public (HeldWeaponState, bool) StateStatus()

    {
        foreach (IAnimator weaponPartObjectAnimator in weaponPartObjectAnimators)
        {
            if (weaponPartObjectAnimator.GetCurrentAnimationName() != null)
                return (state, false);
        }
        return (state, true);
    }
    private void Update()
    {
        if(dirToTarget != Vector3.zero)
        {
            transform.right = reverseLookAt ? -dirToTarget : dirToTarget;
        }
    }
    public List<Vector3> GetFirePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var item in projectileInterfaces)
        {
            positions.Add(item.firePos);
        }
        return positions;
    }
}
public enum HeldWeaponState
{
    Idle = 0,
    ReadyingAttack = 1,
    Attacking = 2,
    ReturnToIdle = 3,
}
