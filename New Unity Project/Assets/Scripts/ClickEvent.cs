using UnityEngine.EventSystems;
using UnityEngine;

public class ClickEvent : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var pos = eventData.pointerCurrentRaycast.gameObject.transform.localPosition;
        if (eventData.pointerId == -2)
        {
            var oldBuilding = eventData.pointerCurrentRaycast.gameObject.GetComponent<Building>();
            var building = Instantiate(oldBuilding.canBeUpgradedTo.GetComponent<Building>());
            PlaceBuilding(building,(int)pos.x,(int)pos.z,BuildingsGrid.Grid);
            building.gameObject.transform.position = pos;

            oldBuilding.Destroy();
            building.Activate();
        }
    }

    private void PlaceBuilding(Building building, int placeX, int placeY, Building[,] grid)
    {
        for (int x = 0; x < building.Size.x; x++)
        {
            for (int y = 0; y < building.Size.y; y++)
            {
                grid[placeX + x, placeY + y] = building;
            }
        }

    }
}
