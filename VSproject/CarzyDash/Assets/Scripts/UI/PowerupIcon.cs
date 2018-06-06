using UnityEngine;
using UnityEngine.UI;

public class PowerupIcon : MonoBehaviour {
    [HideInInspector]
    public Consumable linkedConsumable;

    public Image icon;
    public Slider slider;

    private void Start()
    {
        icon.sprite = linkedConsumable.icon;
    }

    private void Update()
    {
        slider.value = 1.0f - linkedConsumable.timeActive / linkedConsumable.duration;
    }
}
