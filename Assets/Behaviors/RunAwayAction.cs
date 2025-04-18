using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RunAway", story: "[Self] runs away from [Player] using [NavMeshAgent]", category: "Action", id: "3a4bf616570304f0727adcca779ecf3a")]
public partial class RunAwayAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    private GameObject self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    private GameObject player;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMeshAgent;
    private NavMeshAgent navMeshAgent;

    protected override Status OnStart()
    {
        self = Self.Value;
        player = Player.Value;
        navMeshAgent = NavMeshAgent.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 directionAwayFromPlayer = (self.transform.position - player.transform.position).normalized;
        Vector3 newDest = self.transform.position + directionAwayFromPlayer * 5;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDest, out hit, 1.0f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        if (navMeshAgent.transform.position == navMeshAgent.destination) {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

