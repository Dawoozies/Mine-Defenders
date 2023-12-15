using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ItemFactory
{
    [CreateAssetMenu(menuName = "Object Material")]
    public class Material : ScriptableObject
    {
        public Color color;
        public float density;
        public float hardness;
        public float pointedModifier;
        public float bladedModifier;
        public float bluntModifier;
    }
}