using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

public class Perseguidor : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>()
        chase();
    }
    void chase()
    {
        agent SerDestination(obetivo.position);
    }
}
