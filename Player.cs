using UnityEngine;
using System.Collections;

/// <summary>
/// Class that handles the players actions:
/// Collecting orbs
/// </summary>
public class Player : MonoBehaviour {

    private GameManager gm;
    public float inRangeDistance = 2f;  // range for player to be able to grab orb

    // Use this for initialization
    void Start () {
        // access to game obj holding gm script
        gm = GameObject.Find("GameManagerGO").GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
        // check for collision with any orb
        for (int i = 0; i < gm.numOrbs; i++)
        {
            // distance between orb and player
            float dist = Vector3.Distance(transform.position, gm.orbs[i].transform.position);
            // check if collision
            if (Mathf.Abs(dist) <= inRangeDistance) // IndexOutOfRangeException???
            {
                // increase number of flockers now and add new into array with target
                gm.numberFlockers++;
                gm.Flock.Add((GameObject)Instantiate(gm.flockerPrefab, new Vector3(gm.player.transform.position.x - 10, 2, gm.player.transform.position.z - 10), Quaternion.identity));
                // give this new flock member the player target
                gm.Flock[gm.numberFlockers - 1].GetComponent<LeaderFollower>().Leader = gm.target;

                // remove that orb and destroy the GameObject
                Destroy(gm.orbs[i]);
                gm.orbs.RemoveAt(i);
            }
        }
        
	
	}
}
