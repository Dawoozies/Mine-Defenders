using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Projectile : ScriptableObject
{
    public float interpolationSpeed;
    public InterpolationType interpolationType;
    public SpriteAnimation inFlightSpriteAnimation;
    public SpriteAnimation onImpactSpriteAnimation;
}
