using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;
public class JuicyInteract : MonoBehaviour
{
    ContainerCounter counter;
    public float scaleUp = 1.2f;
    public float time = 0.1f;
    void Awake(){
        counter = GetComponentInParent<ContainerCounter>();
        counter.OnPlayerGrabbedObject += OnInteract;  
    } 
    
    void OnInteract(object sender, System.EventArgs e){
        //Debug.Log("Player grabbed object");
        //scale up 1.2 then scale down to 1
        transform.DOScale(Vector3.one * scaleUp, time).OnComplete(() => {
            transform.DOScale(Vector3.one, time);
        });
    }
}
