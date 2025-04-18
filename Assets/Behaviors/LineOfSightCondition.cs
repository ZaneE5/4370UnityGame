using System;
using Unity.Behavior;
using UnityEditor.UI;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line Of Sight", story: "[Camera] can see [Target] using [Renderer]", category: "Conditions", id: "6e1e381283f959e598aa8db8fa275f75")]
public partial class LineOfSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Camera> Camera;
    private Camera cam;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    private GameObject target;
    [SerializeReference] public BlackboardVariable<Renderer> Renderer;
    private Renderer renderer;
    private LayerMask obstacleMask;
    private bool seen;

    //returns true if the camera has LOS to the target with an layer mask of everything
    public override bool IsTrue()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Collider objCollider = target.GetComponentInChildren<Collider>();

        if (!GeometryUtility.TestPlanesAABB(planes, objCollider.bounds)) 
        {
            // Debug.Log("Stalker collider is not visible");
            return false;
        }
        
        Vector3 directionToAgent = (target.transform.position - cam.transform.position).normalized;
        float distanceToAgent = Vector3.Distance(cam.transform.position, target.transform.position);

        Debug.DrawRay(cam.transform.position, directionToAgent, Color.red);

        if (Physics.Raycast(cam.transform.position, directionToAgent, out RaycastHit hit, distanceToAgent, obstacleMask))
        {

            if(hit.transform.tag == target.transform.tag)
            {
                // Debug.Log("Player can see Stalker");
                return true;
            }
            else
            {
                // Debug.Log("Seeing " + hit.transform.name + " instead of Stalker");
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    public override void OnStart()
    {
        cam = Camera.Value;
        target = Target.Value;
        obstacleMask = ~0; //everything
        renderer = Renderer.Value;
    }

    public override void OnEnd()
    {
    }
}
