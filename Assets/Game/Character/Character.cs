using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : ScriptableObject
{
    //Factory pattern input
    //Constructs characters and character animations
    //Basically instead of using prefabs of characters we'll use a standardised
    //construction of them via the character generators
    //Only construct them once at game start and then just instantiate refs to them
}
//Instead of using object animations
//Should use something a bit custom to characters
[Serializable]
public class CharacterAnimation
{
    //Body sprite
    //Hand positions
    //sprite as key
    //Sprite Key Animation
    //Dictionary<Sprite, List<>>
}
//The animations should be modular hmm
//Making scriptable objects for properties and sprites?
//I.e. single frame animations
[CreateAssetMenu]
public class SpriteNode : ScriptableObject
{
    public Sprite sprite;
    public InterpolationNode interpolationNode;
}
//Scriptable Object version
//Won't be the version used during runtime construction?
[Serializable]
public class InterpolationNode
{
    public List<Property> properties;
}
[CreateAssetMenu]
public class InterpolationNodeConstructor : ScriptableObject
{
    public List<Property> properties;

}
[Serializable]
public struct Property
{
    public PropertyType propertyType;
    public InterpolationType interpolationType;
    public Vector3 vector;
    public Quaternion rotation;
}
[Serializable]
public enum PropertyType
{
    ScreenPosition,
    WorldPosition,
    LocalPosition,
    EulerAngles,
    LocalScale,
}
public struct StartProperties
{
    public Vector3 worldPosition;
    public Vector3 localPosition;
    public Vector3 localScale;
    [HideInInspector]
    public Quaternion rotation;
}
public class Interpolator
{
    //Need to link
    Transform obj;
    InterpolationNode currentNode;
    StartProperties initial;
    public Interpolator(Transform obj) { this.obj = obj; }
    public void Interpolate(InterpolationNode node, float time)
    {
        //This doesn't handle going to next node
        if(currentNode != node)
        {
            //This will only happen if time == 0
            //We can't realistically do a check for when time == 0
            currentNode = node;
            initial.worldPosition = obj.position;
            initial.localPosition = obj.localPosition;
            initial.localScale = obj.localScale;
            initial.rotation = obj.rotation;
        }
    }
}
