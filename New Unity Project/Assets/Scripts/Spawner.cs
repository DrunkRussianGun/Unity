using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Spawner : MonoBehaviour
{
    [FormerlySerializedAs("SpawnPosition")] public Transform spawnPosition;
    [FormerlySerializedAs("SpawnObject")] public GameObject spawnObject; 
    [FormerlySerializedAs("TimeForNewSpawn")] public float timeForNewSpawn;

    void Start()
    {
        StartCoroutine(SpawnCd());
    }

    void Repeat()
    {
        StartCoroutine(SpawnCd());
    }
    
    IEnumerator SpawnCd()
    {
    	yield return new WaitForSeconds(timeForNewSpawn);

		if (KabanManager.Instance.Kabans.Count < BuildingManager.Instance.Buildings.Count * 2)
        	Instantiate(spawnObject, spawnPosition.position, spawnPosition.rotation);

    	Repeat();
    }
}
