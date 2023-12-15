using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldWeapon : MonoBehaviour
{
    HeldWeaponState state;
    public List<ObjectAnimator> weaponPartObjectAnimators;
    public Vector3 dirToTarget;
    public bool reverseLookAt;
    public void SetWeaponState(HeldWeaponState state, float stateSpeed)
    {
        //We don't want to be able to move states while attacking
        //We should have a cancel method which can push the weapon out of attacking
        //that is IF we end up needing to cancel for any reason
        if (this.state == state)
            return;
        this.state = state;
        string stateName = state.ToString();
        foreach (ObjectAnimator weaponPartObjectAnimator in weaponPartObjectAnimators)
        {
            weaponPartObjectAnimator.PlayAnimation(stateName);
            weaponPartObjectAnimator.animationSpeed = stateSpeed;
        }
    }
    public (HeldWeaponState, bool) StateStatus()
    {
        foreach (ObjectAnimator weaponPartObjectAnimator in weaponPartObjectAnimators)
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
}
public enum HeldWeaponState
{
    Idle = 0,
    ReadyingAttack = 1,
    Attacking = 2,
    ReturnToIdle = 3,
}
