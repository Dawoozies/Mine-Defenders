using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAndShield : HeldWeapon
{
    public Vector3 swordHoldPos;
    public Transform sword;
    ObjectAnimator swordAnimator;
    public Transform shield;
    ObjectAnimator shieldAnimator;
    void Start()
    {
        weaponPartObjectAnimators = GetComponentsInChildren<IAnimator>();
        swordAnimator = sword.GetComponent<ObjectAnimator>();
        shieldAnimator = shield.GetComponent<ObjectAnimator>();
    }
    public override void SetWeaponState(HeldWeaponState state, float stateSpeed)
    {
        ObjectAnimation sword_ReadyingAttack = new ObjectAnimation();
        sword_ReadyingAttack.animName = "ReadyingAttack";
        sword_ReadyingAttack.frames = 2;
        sword_ReadyingAttack.positions = new List<Vector3> {
            swordHoldPos,
            _dirToTarget * -0.35f
        };
        sword_ReadyingAttack.interpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp };
        swordAnimator.CreateAnimation(sword_ReadyingAttack);
        ObjectAnimation sword_Attacking = new ObjectAnimation();
        sword_Attacking.animName = "Attacking";
        sword_Attacking.frames = 2;
        sword_Attacking.positions = new List<Vector3> {
            _dirToTarget * -0.35f,
            _target.args.worldPos
        };
        sword_Attacking.interpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp };
        swordAnimator.CreateAnimation(sword_Attacking);
        base.SetWeaponState(state, stateSpeed);
    }
    void Update()
    {
        
    }
}
