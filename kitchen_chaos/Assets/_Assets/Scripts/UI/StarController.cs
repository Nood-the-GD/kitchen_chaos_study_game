using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarController : MonoBehaviour
{
    [SerializeField] private List<GameObject> starList = new List<GameObject>();
    [SerializeField] private List<TextMeshProUGUI> pointList = new List<TextMeshProUGUI>();
    [SerializeField] private Sprite _starSprite, _nonStarSprite;

    public void SetData(StageData stageData)
    {
        if (starList.Count > 0)
            ShowStar(stageData.star);
        if(pointList.Count > 0)
            ShowPoint(stageData.pointTarget);
    }

    public void ShowPoint(int[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            pointList[i].text = points[i].ToString();
        }
    }

    public void ShowStar(int star)
    {
        for (int i = 0; i < starList.Count; i++)
        {
            Image starImage = starList[i].GetComponent<Image>();
            if(i < star)
            {
                // Show star
                starImage.sprite = _starSprite;    
            }
            else
            {
                // Show non star
                starImage.sprite = _nonStarSprite;
            }
        }
    }
}
