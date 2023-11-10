using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ItemFactory
{
    [CreateAssetMenu]
    public class Equipment : ScriptableObject
    {
        public Sprite materialPart; //part which is greyscale
        public Sprite basePart; //part which isnt
    }
}