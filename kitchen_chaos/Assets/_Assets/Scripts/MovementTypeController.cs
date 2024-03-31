using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTypeController : MonoBehaviour
{
    public static MovementTypeController Instance{ get; private set; }


    void Awake()
    {
        Instance = this;
    }

    public bool isMobileController;
}
