using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public bool hasGameStarted;
    
    public GameObject go;

    // Update is called once per frame
    void Update()
    {
        if (Bank.Instance.money >= 100 && Bank.Instance.food >= 1000)
        {
            go.SetActive(true);
        }
    }
}
