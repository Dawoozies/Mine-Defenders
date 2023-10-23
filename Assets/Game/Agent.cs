using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }
    public void MoveToWorldPoint(Vector3 target)
    {
        target.z = 0;
        //Debug.Log(target);
        agent.SetDestination(target);
    }
    private void Update()
    {
        if (agent == null)
            return;

        
    }
}
