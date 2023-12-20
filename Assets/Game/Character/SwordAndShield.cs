using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAndShield : HeldWeapon
{
    public Vector3 swordHoldPos;
    public Vector3 shieldHoldPos;
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
        foreach (HeldWeaponState item in Enum.GetValues(typeof(HeldWeaponState)))
        {
            Vector3 swordStartPos = Vector3.zero;
            Vector3 swordEndPos = Vector3.zero;
            Vector3 shieldStartPos = Vector3.zero;
            Vector3 shieldEndPos = Vector3.zero;
            Quaternion swordStartRot = Quaternion.FromToRotation(Vector3.right, Vector3.up);
            Quaternion swordEndRot = Quaternion.FromToRotation(Vector3.right, Vector3.up);
            List<InterpolationType> swordInterpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp };
            if (item == HeldWeaponState.Idle)
            {
                swordStartPos = swordHoldPos;
                swordEndPos = swordHoldPos;
                shieldStartPos = shieldHoldPos;
                shieldEndPos = shieldHoldPos;
            }
            else if(item == HeldWeaponState.ReadyingAttack)
            {
                swordStartPos = swordHoldPos;
                swordEndPos = swordHoldPos + (-_dirToTarget*0.35f);
                shieldStartPos = shieldHoldPos;
                shieldEndPos = shieldHoldPos + (_dirToTarget*0.125f);
                swordStartRot = Quaternion.FromToRotation(Vector3.right, Vector3.up);
                swordEndRot = Quaternion.FromToRotation(Vector3.right,_dirToTarget);
            }
            else if(item == HeldWeaponState.Attacking)
            {
                swordStartPos = swordHoldPos + (-_dirToTarget * 0.35f);
                swordEndPos = _dirToTarget*0.5f;
                shieldStartPos = shieldHoldPos + (_dirToTarget * 0.125f);
                shieldEndPos = shieldHoldPos + (-_dirToTarget * 0.25f);
                swordStartRot = Quaternion.FromToRotation(Vector3.right,_dirToTarget);
                swordEndRot = Quaternion.FromToRotation(Vector3.right,-_dirToTarget);
                swordAnimator.onAnimationComplete += () => {
                    if(swordAnimator.GetCurrentAnimationName() == "Attacking")
                    {
                        _target.args.AttackAgent(_owner, 2);
                        Debug.LogError("Damage Dealt");
                    }
                };
            }
            else if(item == HeldWeaponState.ReturnToIdle)
            {
                swordStartPos = _dirToTarget*0.5f;
                swordEndPos = swordHoldPos;
                shieldStartPos = shieldHoldPos + (-_dirToTarget * 0.25f);
                shieldEndPos = shieldHoldPos;
                swordInterpolationTypes = new List<InterpolationType> { InterpolationType.EaseInExp, InterpolationType.EaseInExp };
                swordStartRot = Quaternion.FromToRotation(Vector3.right, -_dirToTarget);
                swordEndRot = Quaternion.FromToRotation(Vector3.right, -Vector3.up);
            }

            ObjectAnimation swordAnim = new ObjectAnimation();
            swordAnim.animName = item.ToString();
            swordAnim.frames = 2;
            swordAnim.positions = new List<Vector3> { swordStartPos, swordEndPos };
            swordAnim.rotations = new List<Quaternion> { swordStartRot, swordEndRot };
            swordAnim.interpolationTypes = swordInterpolationTypes;
            swordAnimator.CreateAnimation(swordAnim);

            ObjectAnimation shieldAnim = new ObjectAnimation();
            shieldAnim.animName = item.ToString();
            shieldAnim.frames = 2;
            shieldAnim.positions = new List<Vector3> { shieldStartPos, shieldEndPos };
            shieldAnim.interpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp };
            shieldAnimator.CreateAnimation(shieldAnim);
        }
        base.SetWeaponState(state, 4f);
    }
    void Update()
    {
        
    }
}
