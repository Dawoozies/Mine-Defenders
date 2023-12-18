using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager ins;
    private void Awake()
    {
        ins = this;
    }
    public GameObject projectilePrefab;
    public int poolSize;
    List<Projectile> projectiles = new List<Projectile>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject clone = Instantiate(projectilePrefab, transform);
            projectiles.Add(new Projectile(clone));
        }
    }
    public Projectile FireProjectileAtPosition(Vector3 startPos, Vector3 targetPos, float interpolationSpeed, InterpolationType interpolationType, bool lookAtPos)
    {
        foreach (var item in projectiles)
        {
            if(item.inPool)
            {
                item.FireProjectileAtPosition(startPos, targetPos, interpolationSpeed, interpolationType, lookAtPos);
                item.inPool = false;
                return item;
            }
        }
        return null;
    }
    public void ReturnProjectileToPool(Projectile projectile)
    {
        foreach (Projectile item in projectiles)
        {
            if(item == projectile)
            {
                projectile.ReturnToPool();
                return;
            }
        }
    }
}
public class Projectile
{
    public bool inPool;
    public Transform transform;
    public SpriteRenderer[] spriteRenderers;
    public ObjectAnimator objectAnimator;
    public Projectile(GameObject prefab)
    {
        transform = prefab.transform;
        spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>();
        objectAnimator = prefab.GetComponentInChildren<ObjectAnimator>();
        inPool = true;
        foreach (SpriteRenderer item in spriteRenderers)
        {
            item.color = Color.clear;
        }
    }
    public void FireProjectileAtPosition(Vector3 startPos, Vector3 targetPos, float interpolationSpeed, InterpolationType interpolationType, bool lookAtPos)
    {
        objectAnimator.animationSpeed = interpolationSpeed;
        foreach (SpriteRenderer item in spriteRenderers)
        {
            item.color = Color.white;
        }
        if (lookAtPos)
            transform.right = -(targetPos - startPos);
        ObjectAnimation anim = new ObjectAnimation();
        anim.animName = "MoveToPosition";
        anim.frames = 2;
        anim.useWorldPosition = true;
        anim.positions = new List<Vector3> { 
            startPos,
            targetPos
        };
        anim.interpolationTypes = new List<InterpolationType> { interpolationType, interpolationType };
        objectAnimator.CreateAndPlayAnimation(anim);
    }
    public void ReturnToPool()
    {
        foreach (SpriteRenderer item in spriteRenderers)
        {
            item.color = Color.clear;
        }
        inPool = true;
    }
}