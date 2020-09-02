using System.Collections;
using System.Collections.Generic;
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
    	Instantiate(SpawnObject, SpawnPosition.position, Quaternion.identity);
    	Repeat();
    }
}
