using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Generic Vehicle class which must have subclasse

[RequireComponent(typeof(CharacterController))]

abstract public class Vehicle : MonoBehaviour {
    
	protected Vector3 acceleration;
	protected Vector3 velocity;
	protected Vector3 desired;

    public float maxSpeed = 15.0f;
    public float maxForce = 20.0f;
    public float mass = 1.0f;
    public float radius = 1.0f;
    public float slowingRadius = 30.0f;

    private float LEADER_BEHIND_DIST = 30f;
    private float LEADER_SIGHT_RADIUS = 5f;

    CharacterController charControl;

    //access to game manager script
    protected GameManager gm;

	public Vector3 Velocity {
		get { return velocity; }
	}    

	virtual public void Start(){
		acceleration = Vector3.zero;
		velocity = transform.forward;
		desired = Vector3.zero;

		//store access to character controller component
		charControl = GetComponent<CharacterController>();

		// access to game obj holding gm script
		gm = GameObject.Find("GameManagerGO").GetComponent<GameManager>();
	}
    	
	// Update is called once per frame
	protected void Update () {
		CalcSteeringForces ();

		//add accel to vel
		velocity += acceleration * Time.deltaTime;
		velocity.y = 0;
		//limit velo to max speed
		velocity = Vector3.ClampMagnitude (velocity, maxSpeed);
		//dude face torward target
		transform.forward = velocity.normalized;
		//added vel to pos to move
		charControl.Move (velocity * Time.deltaTime);
		//reset accel
		acceleration = Vector3.zero;

	}

    abstract protected void CalcSteeringForces();

    protected void ApplyForce(Vector3 steeringForce){
		acceleration += steeringForce / mass;
	}

/////////////////////////
// STEERING ALGORITHMS //
/////////////////////////
    #region

    protected Vector3 Seek(Vector3 targetPosition){ 
		desired = Vector3.Normalize(targetPosition - transform.position) * maxSpeed;
		Vector3 seekingForce = desired - velocity;
		seekingForce.y = 0; 
		return seekingForce; 
	}

    protected Vector3 Flee(Vector3 targetPosition)
    {
        return Seek(targetPosition) * -1;
    }

    protected Vector3 Arrive(Vector3 targetPosition)
    {
        desired = targetPosition - transform.position;
        float distance = desired.magnitude;

        // check the distance to detect whether the character is inside the slowing area
        if (distance < slowingRadius)
        {
            // inside slowing area
            desired = desired.normalized * maxSpeed * (distance / slowingRadius);
        }
        else
        {
            // outside slowing area
            desired = desired.normalized * maxSpeed;
        }

        Vector3 arriveForce = desired - velocity;
        arriveForce.y = 0;
        return arriveForce;
    }

    protected Vector3 Pursue(Vehicle target)
    {
        int timeAhead = 15;
        Vector3 futurePos = target.transform.position + target.velocity * timeAhead;
        return Seek(futurePos);
    }

    protected Vector3 Evade(Vehicle from)
    {
        Vector3 dist = from.transform.position - this.transform.position;
        int updatesAhead = (int)(dist.magnitude / maxSpeed);
        Vector3 futurePosition = from.transform.position + from.velocity * updatesAhead;
        return Flee(futurePosition);
    }

    protected Vector3 LeaderFollow(Vehicle leader)
    {
        // calculate negative velocity vector of leader
        Vector3 tv = leader.velocity * -1;
        tv.Normalize();

        // get behind vector
        Vector3 behind = leader.transform.position + tv;

        // calculate the ahead vector
        Vector3 tv2 = leader.velocity;
        tv = tv.normalized * LEADER_BEHIND_DIST;
        Vector3 ahead = leader.transform.position + tv2;

        Vector3 followForce = Arrive(behind);

        // if the character is on the leader's sight, add an evade
        if (isOnLeaderSight(leader, ahead))
        {
            followForce += Evade(leader);
        }
       
        return followForce;
    }

    protected bool isOnLeaderSight(Vehicle leader, Vector3 leaderAhead)
    {
        return Vector3.Distance(leaderAhead, this.transform.position) <= LEADER_SIGHT_RADIUS || Vector3.Distance(leader.transform.position, this.transform.position) <= LEADER_SIGHT_RADIUS;
    }

	protected Vector3 AvoidObstacle(GameObject obst, float sD){
		desired = Vector3.zero;
		//distance from dude to obstacle
		Vector3 vecToCenter = obst.transform.position - transform.position;
		vecToCenter.y = 0;
		//radius of obstacle
		float obstRad = obst.GetComponent<ObstacleScript> ().Radius;
		//calculate safe distance
		if (vecToCenter.magnitude > sD) {
			return Vector3.zero;
		}
		//check obstacle behind
		if (Vector3.Dot (vecToCenter, transform.forward) < 0) {
			return Vector3.zero;
		}
		//will it not collide?
		if (Mathf.Abs (Vector3.Dot (vecToCenter, transform.right)) > obstRad + radius) {
			return Vector3.zero;
		}
		//will it collide?
		//is it on your left or right?
		if(Vector3.Dot(vecToCenter, transform.right) < 0){ //on left move right
			desired = transform.right * maxSpeed;
			//debug line to see if the dude is avoiding to the right
			//Debug.DrawLine(transform.position, obst.transform.position, Color.red);
			Debug.DrawLine(obst.transform.position, transform.position, Color.red);
		}
		else { //on right move left
			desired = transform.right * -maxSpeed;
			//debug line to see if the dude is avoiding to the left
			Debug.DrawLine(transform.position, obst.transform.position, Color.green);
		}
		return desired;
	}


	 ///<summary>
	 ///Separation: Keeps flockers seperated by a certain distance from each other
	 ///</summary>
	public Vector3 Separation()
	{
		float desiredSeparation = 25.0f;
		Vector3 steer = Vector3.zero;
		int count = 0;

		foreach (GameObject gO in gm.Flock)
		{
			float dist = Vector3.Distance(transform.position, gO.transform.position);

			// if the distance is greater than 0 and less than an arbitrary about (0 is yourself)
			if ((dist > 0) &&(dist < desiredSeparation))
			{
				// calculate the vector pointing away from teh neighbor
				Vector3 diff = transform.position - gO.transform.position;
				diff.Normalize();
				diff = diff / dist;
				steer += diff;
				count++;
			}
		}
		
		// get average
		if (count > 0)
		{
			steer /= (float)count;
		}
		
		if (steer.magnitude > 0) {
			steer.Normalize();
			steer *= maxSpeed;
			steer -= velocity;
			steer = Vector3.ClampMagnitude(steer, maxForce);
		}
		return steer;

	}

	/// <summary>
	/// Alignment
	/// </summary>
	/// <param name="alignVector">The vector which with the vehicle will align itself</param>
	public Vector3 Alignment(Vector3 alignVector)
	{
		Vector3 vtc = Vector3.zero;
		vtc = Vector3.Normalize (alignVector - transform.position);
		return vtc;
	}

	public Vector3 Cohesion(Vector3 cohesionVector){
		Vector3 vtc = Vector3.zero;
		vtc = Vector3.Normalize (cohesionVector - transform.position);
		return vtc;
	}
    

    #endregion
}
