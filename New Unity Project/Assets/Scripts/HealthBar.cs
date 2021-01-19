using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    protected Slider Slider => GetComponent<Slider>();

    protected virtual void Start()
    {
        // Debug.LogWarning("Slider: " + slider + ", exists: " + (bool)slider);
    }

    public void SetHealth(int health)
    {
        Slider.value = health;
        var isHealthBarVisible = Slider.value < Slider.maxValue;
        foreach (var @object in gameObject.GetComponentsInChildren<Image>())
            @object.enabled = isHealthBarVisible;
    }

    public void SetMaxHealth(int health)
    {
        Slider.maxValue = health;
        Slider.value = health;
    }
}
