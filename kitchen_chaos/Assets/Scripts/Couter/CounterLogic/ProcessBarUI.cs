using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessBarUI : MonoBehaviour
{
    [SerializeField] private CuttingCounter cuttingCounter;
    [SerializeField] private Image barImage;

    private void Start()
    {
        cuttingCounter.OnProcessChanged += CuttingCounter_OnProcessChanged;
        barImage.fillAmount = 0;

        HideBar();
    }

    private void CuttingCounter_OnProcessChanged(object sender, CuttingCounter.OnProcessChangedEvenArgs e)
    {
        barImage.fillAmount = e.processNormalize;

        if(e.processNormalize == 1f)
        {
            HideBar();
        }
        else
        {
            ShowBar();
        }
    }

    public void ShowBar()
    {
        gameObject.SetActive(true);
    }

    public void HideBar()
    {
        gameObject.SetActive(false);
    }
}
