using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject shopPanel;

    public void OpenPanel() 
    {
    	if(shopPanel != null) 
    	{
    		shopPanel.SetActive(true);
    	}
    }

    public void ClosePanel() 
    {
    	shopPanel.SetActive(false);
    }
}
