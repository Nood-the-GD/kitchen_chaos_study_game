using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Variables
    public static SoundManager Instance {get; private set;}
    [SerializeField] private AudioClipRefSO audioClipRef;
    #endregion

    #region Unity functions
    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
    void OnEnable()
    {
        Player.OnPlayerSpawn += GameManager_OnPlayerSpawn;
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnCut += CuttingCounter_OnCut;
        BaseCounter.OnSomethingPlacedHere += BaseCounter_OnSomethingPlaceHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        PlatesCounter.OnAnyPlateSpawn += PlatesCounter_OnPlateSpawn;

    }
    void OnDisable()
    {
        Player.OnPlayerSpawn -= GameManager_OnPlayerSpawn;
    }
    #endregion

    #region GameManager events
    private void GameManager_OnPlayerSpawn(Player e)
    {
        Player.Instance.OnPickupSomething += Player_OnPickupSomething;
    }
    #endregion

    #region Counter event
    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRef.trash, trashCounter.transform.position);
    }

    private void PlatesCounter_OnPlateSpawn(object sender, System.EventArgs e)
    {
        PlatesCounter platesCounter = sender as PlatesCounter;
        PlaySound(audioClipRef.plate, platesCounter.transform.position);
    }

    private void CuttingCounter_OnCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRef.chop, cuttingCounter.transform.position);
    }
    private void BaseCounter_OnSomethingPlaceHere(object sender, System.EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(audioClipRef.objectDrop, baseCounter.transform.position);
    }
    #endregion

    #region Player events
    private void Player_OnPickupSomething(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRef.objectPickup, Player.Instance.transform.position);
    }
    #endregion

    #region Delivery events
    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRef.deliverySuccess, DeliveryCounter.Instance.transform.position);
    }
    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRef.deliveryFail, DeliveryCounter.Instance.transform.position);
    }
    #endregion


    #region Play sound
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, 1);
    }
    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, 1);
    }
    public void PlayFootstepSound(Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipRef.footStep, position);
    }
    #endregion
}
