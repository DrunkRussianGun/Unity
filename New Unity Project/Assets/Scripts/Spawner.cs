using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform SpawnPosition;
    public GameObject SpawnObject; 
    public float TimeForNewSpawn;

    void Start()
    {
        StartCoroutine(SpawnCD());
    }

    void Repeat()
    {
        StartCoroutine(SpawnCD());
    }
    
    IEnumerator SpawnCD()
    {
    	yield return new WaitForSeconds(TimeForNewSpawn);

		if (KabanManager.instance.kabans.Count < BuildingManager.instance.buildings.Count * 2)
        	Instantiate(SpawnObject, SpawnPosition.position, SpawnPosition.rotation);

    	Repeat();
    }
}
