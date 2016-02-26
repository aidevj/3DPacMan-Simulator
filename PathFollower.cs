using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Using elements of the Seeker Class (non-flock) to follow  set path (set by gizmos in unity)
/// Used for ghosts
/// </summary>
public class PathFollower : Vehicle
{
    private GameObject[] wpPath; // private so now overridden by "None"'s in inspector?
    //public string wpTag;    // get the correct tagged path
    public int pathID;

    private Vector3 force;  // Ultimate steering force

    public float seekWeight = 75.0f;
    public float avoidWeight = 30.0f;
    
    // Path Following variables
    public float reachDistance = 2.0f;  // distance at which the vehicle has "arrived" at a WP
    private int currentPoint = 0;  // initial WP

    public float safeDistance = 30.0f; // from obstacles and player character
    public float pursueWeight = 200.0f;

    private Vehicle playerTarget;
    //private bool targetting;  // this doesn't not necessarily need to be used


    // Call Inherited Start and then do our own
    override public void Start()
    {
        base.Start();

        // Initialize ultimate force vector
        force = Vector3.zero;

        // Initialize pursuit variables
        playerTarget = gm.player.GetComponent<Vehicle>();
        //targetting = false;

        // Assign path for player to follow (put the transforms into the player's path follow Transform array)
        switch (pathID){
            case 0: // test pathman
                wpPath = gm.path0;
                break;
            case 1: // clyde
                wpPath = gm.path1;
                break;
            case 2:
                wpPath = gm.path2;
                break;
            case 3:
                wpPath = gm.path3;
                break;
        }
    }


    // Draw gizmos to show a defined path
    // Editor function that will draw spheres to edit path within Unity, will not show up in gameplay
    public void OnDrawGizmos()
    {
        for (int i = 0; i < wpPath.Length; i++)
        {
            // Check the path does not exist (empty array)
            if (wpPath.Length > 0) // nothing will draw

                // check that path exists
                if (wpPath[i] != null)
                {
                    // draw gizmos
                    Gizmos.DrawSphere(wpPath[i].transform.position, reachDistance);
                }
        }
    }

    protected override void CalcSteeringForces()
    {
        //reset ultimate force
        force = Vector3.zero;   

        /////////////////// Chase Player
        // check if pacman is in range
        Vector3 vecToCen = Vector3.zero;
        vecToCen = playerTarget.transform.position - this.transform.position;
        float vtcDist = vecToCen.magnitude;

        // check if distance from vecToC to pacman is smaller than safedistance
        if (Mathf.Abs(vtcDist) < safeDistance)
        {
            // set targetting to true
            //targetting = true;
            maxSpeed = 17f; // speed up upon pursual
            force += Pursue(playerTarget) * pursueWeight;
            Debug.DrawLine(this.transform.position, playerTarget.transform.position, Color.cyan);
        }
        else // pacman is not in range, resume path finding
        {
            //targetting = false;
            maxSpeed = 10f; //default ghost speed

            /////////// Path finding
            // get distance between vehicle and waypoint
            float dist = Vector3.Distance(wpPath[currentPoint].transform.position, this.transform.position);

            // now add the seek force going towards that waypoint
            force += Seek(wpPath[currentPoint].transform.position) * seekWeight;

            Debug.Log("currentPoint = " + currentPoint);
            Debug.Log("Distance = " + dist);

            // check if the magnitude of the direction is within range of the next waypoint
            if (dist <= reachDistance)
            {
                Debug.Log("To next point");
                // move onto seeking the next waypoint now
                currentPoint++;
            }

            // check if all waypoints have been covered
            if (currentPoint >= wpPath.Length)
            {
                currentPoint = 0; // reset
            }
        }
        
        /////////////////// Avoiding obstacles
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
