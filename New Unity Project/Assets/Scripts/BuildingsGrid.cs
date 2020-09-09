using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsGrid : MonoBehaviour
{
    public Vector2Int GridSize = new Vector2Int(10, 10);

    public static Building[,] grid;
    private Building flyingBuilding;
    private Camera mainCamera;

    private void Awake()
    {
        grid = new Building[GridSize.x, GridSize.y];

        mainCamera = Camera.main;
    }

    public void StartPlacingBuilding(Building buildingPrefab)
    {
        if (!(flyingBuilding is null))
	        CancelPlacingBuilding();

        if (Bank.Instance.money < buildingPrefab.cost)
	        return;

        Bank.Instance.money -= buildingPrefab.cost;
        flyingBuilding = Instantiate(buildingPrefab);
    }

    // Update is called once per frame
    void Update()
    {
	    if (flyingBuilding is null)
		    return;
	    if (Input.GetMouseButtonDown(1))
	    {
		    CancelPlacingBuilding();
		    return;
	    }

	    var groundPlane = new Plane(Vector3.up, Vector3.zero);

	    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
	    if (groundPlane.Raycast(ray, out float position))
	    {
		    Vector3 wrldPos = ray.GetPoint(position);

		    int x = Mathf.RoundToInt(wrldPos.x);
		    int y = Mathf.RoundToInt(wrldPos.z);

		    flyingBuilding.transform.position = new Vector3(x,0,y);

		    bool available = true;
		    if (x < 0 || x > GridSize.x - flyingBuilding.Size.x) available = false;
		    if (y < 0 || y > GridSize.y - flyingBuilding.Size.y) available = false;
		    if (available && IsPlaceTaken(x, y)) available = false;
		    if (available  && Input.GetMouseButtonDown(0))
		    {
			    PlaceFlyingBuilding(x, y);
		    }
	    }
    }

    private void CancelPlacingBuilding()
    {
	    Bank.Instance.money += flyingBuilding.cost;
	    Destroy(flyingBuilding.gameObject);
	    flyingBuilding = null;
    }
    
    private bool IsPlaceTaken(int placeX, int placeY)
    {
        for (int x = 0; x < flyingBuilding.Size.x; x++)
        {
            for (int y = 0; y < flyingBuilding.Size.y; y++)
            {
                if (grid[placeX + x, placeY + y] != null) return true;
            }
        }

        return false;
    }

    private void PlaceFlyingBuilding(int placeX, int placeY)
    {
        for (int x = 0; x < flyingBuilding.Size.x; x++)
        {
            for (int y = 0; y < flyingBuilding.Size.y; y++)
            {
                grid[placeX + x, placeY + y] = flyingBuilding;
            }
        }
        
		flyingBuilding.Activate();
        flyingBuilding = null;
    }
}
