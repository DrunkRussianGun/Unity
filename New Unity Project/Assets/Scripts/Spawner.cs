using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform SpawnPosition;
    public GameObject SpawnObject; 
    public float TimeForNewSpawn;
    
    private List<GameObject> justSpawnedKabans = new List<GameObject>();

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
        CreateKaban();
    	Repeat();
    }

    private void CreateKaban()
    {
        if (BuildingManager.instance.buildings.Count == 0)
            return;
        
        justSpawnedKabans = justSpawnedKabans
            .Where(x => Vector3.Distance(x.transform.position, SpawnPosition.position) < 1f)
            .ToList();
        if (justSpawnedKabans.Count > 0)
            return;
        
    	var kaban = Instantiate(SpawnObject, SpawnPosition.position, SpawnPosition.rotation);
        justSpawnedKabans.Add(kaban);
    }
}
