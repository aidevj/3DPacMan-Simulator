using UnityEngine;
using System.Collections;

/// <summary>
/// In which a vehicle arrives at a point behind a leader
/// </summary>
public class LeaderFollower : Vehicle {

    [HideInInspector]private GameObject leader;

    public GameObject Leader { get { return leader; } set { leader = value; } }

    //ultimate steering force that will be applied to acceleration
    private Vector3 force;

    //define necessary weights for seeker
    public float seekWeight = 75.0f;
    public float safeDistance = 10.0f;
    public float avoidWeight = 100.0f;
    public float sepWeight = 10.0f;
    public float alignWeight = 40.0f;
    public float coWeight = 40.0f;


    // Call Inherited Start and then do our own
    override public void Start()
    {
        base.Start();
        force = Vector3.zero;
    }

    protected override void CalcSteeringForces()
    {
        //reset ultimate force
        force = Vector3.zero;

        //get a seeking force (based on char movement - for now, just seek)
        //add that seeking force to the ultimate steering force
        force += LeaderFollow(leader.GetComponent<Vehicle>()) * seekWeight;

        force += Separation() * sepWeight;
        force += Alignment(gm.FlockDirection) * alignWeight;
        force += Cohesion(gm.Centroid) * coWeight;
        
        // avoiding obstacle
        for (int i = 0; i < gm.Obstacles.Length; i++)
        {
            force += AvoidObstacle(gm.Obstacles[i], safeDistance) * avoidWeight;
        }

        //limit steering force
        force = Vector3.ClampMagnitude(force, maxForce);

        //applyForce to acceleration
        ApplyForce(force);
    }

}

