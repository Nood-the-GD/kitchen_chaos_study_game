using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarController : MonoBehaviour
{
    [SerializeField] List<GameObject> starList = new List<GameObject>();
    [SerializeField] private Sprite _starSprite, _nonStarSprite;

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
