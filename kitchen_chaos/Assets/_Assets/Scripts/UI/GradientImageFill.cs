using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GradientImageFill : MonoBehaviour
{
    [SerializeField] private Gradient gradient;
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void UpdateImageColor()
    {
        if (image == null) return;

        image.color = gradient.Evaluate(image.fillAmount);
    }
}
