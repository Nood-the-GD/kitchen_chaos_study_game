using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressBarGameObject;
    [SerializeField] private Image barImage;

    private IHasProgressBar hasProgressBas;

    private void Start()
    {
        hasProgressBas = hasProgressBarGameObject.GetComponent<IHasProgressBar>();
        if(hasProgressBas == null)
        {
            Debug.LogError("Game object " + hasProgressBarGameObject.name + " do not implement IHasProgressBar");
        }

        hasProgressBas.OnProcessChanged += HasProgress_OnProcessChangedHandler;
        barImage.fillAmount = 0;

        HideBar();
    }

    private void HasProgress_OnProcessChangedHandler(object sender, IHasProgressBar.OnProcessChangedEvenArgs e)
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
