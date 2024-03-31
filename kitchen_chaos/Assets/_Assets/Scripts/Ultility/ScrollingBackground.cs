using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScrollingBackground : MonoBehaviour
{

    RawImage rawImage;
    public float x, y;
    Rect rect;
    // Start is called before the first frame update
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rect = new Rect(0,0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        rect.position += new Vector2(x,y) * Time.deltaTime;
        rect.size = rawImage.uvRect.size;
        rawImage.uvRect = rect;
    }
}
