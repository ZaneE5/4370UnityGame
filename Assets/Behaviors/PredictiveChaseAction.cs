using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PredictiveChase", story: "[Agent] navigates to predicted location of [player]", category: "Action", id: "18fc360db761084256c177f24de4876a")]
public partial class PredictiveChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private GameObject agent;
    private NavMeshAgent navMeshAgent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    private GameObject player;
    private Vector3 previousPlayerPosition;

    protected override Status OnStart()
    {
        agent = Agent.Value;
        player = Player.Value;

        navMeshAgent = agent.GetComponent<NavMeshAgent>();
        previousPlayerPosition = player.transform.position;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 currentPlayerPosition = player.transform.position;
        Vector3 velocity = (currentPlayerPosition - previousPlayerPosition) / Time.deltaTime;
        previousPlayerPosition = currentPlayerPosition;

        //clamp dest to map bounds
        Vector3 dest = currentPlayerPosition + (1 * velocity);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 1.0f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        // Debug.Log("Player velocity: " + velocity + " Player Position: " + currentPlayerPosition + " Agent Destination: " + dest);
        // navMeshAgent.SetDestination(dest);

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

