using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkipTutorialToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _checkImage;
    [SerializeField] private GameObject _description;
    bool isSkipTutorial;

    #region Unity functions
    void OnEnable()
    {
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
        PhotonManager.s.onPlayerEnteredRoom += OnPlayerEnterRoom;

        Deselect();
    }
    void OnDisable()
    {
        if (PhotonManager.s == null) return;
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;

        Deselect();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(isSkipTutorial)
        {
            Deselect();
        }
        else
        {
            Select();
        }
    }
    #endregion

    #region Private
    private void Deselect()
    {
        isSkipTutorial = false;
        if(_checkImage)
            _checkImage.gameObject.SetActive(isSkipTutorial);
        if(_description)
            _description.SetActive(isSkipTutorial);
        CmdSkipTutorial();
    }
    private void Select()
    {
        isSkipTutorial = true;
        _checkImage.gameObject.SetActive(isSkipTutorial);
        _description.SetActive(isSkipTutorial);
        CmdSkipTutorial();
    }
    #endregion

    #region Multiplay
    private void OnPlayerEnterRoom(Photon.Realtime.Player player)
    {
        Deselect();
    }
    private void OnCallAnyCmdFunction(CmdOrder order)
    {
        if(order.receiver != nameof(SkipTutorialToggle)) return;
        if(order.functionName != nameof(SkipTutorial)) return;

        SkipTutorial((bool)order.data[0]);
    }
    private void CmdSkipTutorial()
    {
        var order = new CmdOrder(nameof(SkipTutorialToggle), nameof(SkipTutorial), isSkipTutorial);
        PhotonManager.s.CmdCallFunction(order);
    }
    private void SkipTutorial(bool value)
    {
        Debug.Log("Skip tutorial");
        TutorialData.Instance.SkipTutorial(value);
        Debug.Log(TutorialData.Instance.GetSkipTutorialNumber());
    }
    #endregion
}
