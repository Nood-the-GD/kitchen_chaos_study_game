using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    [SerializeField] List<GameObject> starList = new List<GameObject>();

    public void ShowStar(int star)
    {
        for (int i = 0; i < starList.Count; i++)
        {
            starList[i].SetActive(i < star);
        }
    }
}
