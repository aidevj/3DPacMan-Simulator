using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles gameplay and instantiates objects
/// Must be on an empty game object "Game Manager GO"
/// </summary>
public class GameManager : MonoBehaviour {
    
    // array for path
    public GameObject[] path0;
    public GameObject[] path1;
    public GameObject[] path2;
    public GameObject[] path3;

    // List of orbs
    public List<GameObject> orbs;
    public int numOrbs = 20;

    // Game objects
    // Hidden from inspector, as instantiated through code
    [HideInInspector]public GameObject flocker;
    [HideInInspector]public GameObject player;
    [HideInInspector]public GameObject target;
    [HideInInspector]public GameObject ghostC;
    [HideInInspector]public GameObject ghostP;
    [HideInInspector]public GameObject ghostB;
    [HideInInspector]public GameObject ghostI;
    [HideInInspector]public GameObject orb;

    private GameObject[] obstacles;

	// Prefabs //
    public GameObject flockerPrefab;
	public GameObject playerPrefab;
    public GameObject obstaclePrefab;
    public GameObject ghostCPrefab;
    public GameObject ghostPPrefab;
    public GameObject ghostBPrefab;
    public GameObject ghostIPrefab;
    public GameObject orbPrefab;

    ////////////////////////
    // Flocking Variables //
    ////////////////////////

    private Vector3 centroid;
    private Vector3 flockDirection;
    public int numberFlockers;
    private List<GameObject> flock;

    public Vector3 Centroid { get { return centroid; } }

    public Vector3 FlockDirection { get { return flockDirection; } }

    public List<GameObject> Flock { get { return flock; } }

    public GameObject[] Obstacles { get { return obstacles; } }

    // Use this for initialization
    void Start () {
        // assign obstacles into array
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        //Create the Player
        Vector3 pos_player = new Vector3(206, 2, 201);
        player = (GameObject)Instantiate(playerPrefab, pos_player, Quaternion.identity);
        
        ghostC = (GameObject)Instantiate(ghostCPrefab, new Vector3(283, 0, 229), Quaternion.identity);
        ghostP = (GameObject)Instantiate(ghostPPrefab, new Vector3(289, 0, 230), Quaternion.identity);
        ghostB = (GameObject)Instantiate(ghostBPrefab, new Vector3(217, 0, 181), Quaternion.identity);
        ghostI = (GameObject)Instantiate(ghostIPrefab, new Vector3(323, 0, 155), Quaternion.identity);

        // create orbs at random spots within maze area
        orbs = new List<GameObject>();
        for (int i = 0; i < numOrbs; i++)
        {
            Vector3 orbPos = new Vector3(Random.Range(228, 336), 3, Random.Range(149, 262));
            orb = (GameObject)Instantiate(orbPrefab, orbPos, Quaternion.identity);
            orbs.Add(orb);
        }

        // create flock list
        flock = new List<GameObject>();
        for (int i = 0; i < numberFlockers; i++)
        {
            Vector3 pos2 = new Vector3(Random.Range(211, 220), 2, Random.Range(174, 194));
            flocker = (GameObject)Instantiate(flockerPrefab, pos2, Quaternion.identity);
            flock.Add(flocker);
        }

        // Flock Target (leader follow)
        target = player; // make the target the player character   

        // Assign a target to the seeker component of each flock member
        for (int i = 0; i < numberFlockers; i++)
        {
            flock[i].GetComponent<LeaderFollower>().Leader = target;
        }
        
    }

    // Update is called once per frame
    void Update () {
        CalcCentroid();
    }

    // Calculate centroid
    public void CalcCentroid()
    {
        for (int i = 0; i < numberFlockers; i++)
        {
            centroid += flock[i].transform.position;
            centroid /= flock.Count;
        }
    }

    // add flocking forces
    public void CalcFlockDirection()
    {
        for (int i = 0; i < flock.Count; i++)
        {
            flockDirection += flock[i].transform.forward;
            flockDirection = flockDirection / flock.Count;
            flockDirection = flockDirection.normalized;
        }
    }
}
