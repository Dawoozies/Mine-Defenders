using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour, IProjectile
{
    public Transform arrow;
    SpriteRenderer arrowSpriteRenderer;
    public float arrowLengthOffset;
    public Transform topStringArrowPoint;
    public Transform bottomStringArrowPoint;
    public Vector3 firePos { get { return (topStringArrowPoint.position + bottomStringArrowPoint.position) / 2f; } }
    void Start()
    {
        arrowSpriteRenderer = arrow.GetComponent<SpriteRenderer>();
    }
    public void ReadyProjectile()
    {
        arrowSpriteRenderer.color = Color.white;
    }
    public void ShootProjectile()
    {
        arrowSpriteRenderer.color = Color.clear;
    }
    private void Update()
    {
        arrow.position = firePos;
        arrow.localPosition += new Vector3(arrowLengthOffset, 0, 0);
    }
}
