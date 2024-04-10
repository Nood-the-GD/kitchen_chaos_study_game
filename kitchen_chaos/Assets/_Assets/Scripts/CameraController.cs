using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance{ get; private set; }
    private CinemachineVirtualCamera _virtualCamera;    

    void Awake()
    {
        Instance = this;
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    void OnEnable()
    {
        Player.OnPlayerSpawn += Player_OnPlayerSpawn;
    }
    void Start()
    {
    }
    void OnDisable()
    {
        Player.OnPlayerSpawn -= Player_OnPlayerSpawn;
    }

    private void Player_OnPlayerSpawn(Player e)
    {
        SetFollowTarget(e.transform);
    }

    public void SetLookAtTarget(Transform target)
    {
        _virtualCamera.LookAt = target;        
    }
    public void SetFollowTarget(Transform target)
    {
        _virtualCamera.Follow = target;        
    }
}
