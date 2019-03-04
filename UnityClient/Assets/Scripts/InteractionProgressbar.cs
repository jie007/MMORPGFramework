using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionProgressbar : MonoBehaviour
{
    public Slider Slider;
    public GameObject[] Children;

    public bool IsActive = false;

    public void Initialize(float maxValue)
    {
        this.Slider.maxValue = maxValue / 1000.0f;
        this.Slider.minValue = 0;
        this.Slider.value = 0;
        SetVisibility(true);
    }

    private void SetVisibility(bool isVisible)
    {
        IsActive = isVisible;
        foreach (var child in Children)
        {
            child.SetActive(isVisible);
        }
    }

    public void Update()
    {
        if (!IsActive)
            return;

        this.Slider.value += Time.deltaTime;
        if (this.Slider.value >= this.Slider.maxValue)
        {
            Hide();
        }
    }

    public void Hide()
    {
        SetVisibility(false);
    }
}
