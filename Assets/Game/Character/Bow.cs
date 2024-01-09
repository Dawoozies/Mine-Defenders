using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : HeldWeapon
{
    public Transform arrow;
    SpriteRenderer[] arrowSpriteRenderers;
    public float arrowLengthOffset;
    public Transform topStringArrowPoint;
    public Transform bottomStringArrowPoint;
    public Vector3 firePos { get { return (topStringArrowPoint.position + bottomStringArrowPoint.position) / 2f; } }
    void Start()
    {
        arrowSpriteRenderers = arrow.GetComponentsInChildren<SpriteRenderer>();
        weaponPartObjectAnimators = GetComponentsInChildren<IAnimator>();
    }
    public override void SetWeaponState(HeldWeaponState state, float stateSpeed)
    {
        //We don't want to be able to move states while attacking
        //We should have a cancel method which can push the weapon out of attacking
        //that is IF we end up needing to cancel for any reason
        base.SetWeaponState(state, stateSpeed);
        if (state == HeldWeaponState.ReadyingAttack)
            ReadyProjectile();
        if (state == HeldWeaponState.Attacking)
            ShootProjectile();
    }
    public void ReadyProjectile()
    {
        foreach (var item in arrowSpriteRenderers)
        {
            item.color = Color.white;
        }
    }
    public void ShootProjectile()
    {
        foreach (var item in arrowSpriteRenderers)
        {
            item.color = Color.clear;
        }
        Projectile projectile = ProjectileManager.ins.FireProjectileAtPosition(
                firePos,
                _target.args.worldPos - 0.35f * _dirToTarget,
                4f,
                InterpolationType.EaseOutExp,
                true
            );
        if (projectile == null)
        {
            Debug.LogError("NOT ENOUGH PROJECTILES IN POOL");
        }
        else
        {
            projectile.objectAnimator.onAnimationComplete += () =>
            {
                ProjectileManager.ins.ReturnProjectileToPool(projectile);
                _target.AttackAgent(_owner, 2);
            };
        }
    }
    private void Update()
    {
        if (lookAt && _dirToTarget != Vector3.zero)
        {
            transform.right = reverseLookAt ? -_dirToTarget : _dirToTarget;
        }
        arrow.position = firePos;
        arrow.localPosition += new Vector3(arrowLengthOffset, 0, 0);
    }
}
