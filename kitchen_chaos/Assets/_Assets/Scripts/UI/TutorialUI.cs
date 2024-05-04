using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    #region Events
    public static Action OnTutorialComplete;
    #endregion

    #region Variables
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private List<Sprite> _tutorialSprites = new List<Sprite>();
    [SerializeField] private Image _tutorialImage;
    [SerializeField] private Image _checkImage;
    [SerializeField] private Transform _checkImageHolder;

    private List<Image> _checkImageList = new List<Image>();
    private int _tutorialIndex = 0;
    private StageData _selectedStage;
    #endregion

    #region Unity functions
    private void Start()
    {
        _checkImage.gameObject.SetActive(false);
        _confirmBtn.onClick.AddListener(() =>
        {
            CmdConfirm();
        });
        _selectedStage = GameManager.getStageData;
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
    }
    void OnDestroy()
    {
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;
    }
    #endregion

    #region Multiplayer functions
    void OnCallAnyCmdFunction(CmdOrder order){
        if(order.receiver != nameof(TutorialUI)) return;

        if(order.functionName == nameof(Confirm)){
            Confirm();
        }
    }
    private void Confirm()
    {
        Image checkImage = Instantiate(_checkImage, _checkImageHolder);
        checkImage.gameObject.SetActive(true);
        _checkImageList.Add(checkImage);

        _confirmBtn.gameObject.SetActive(false);

        if(_checkImageList.Count == 2)
        {
            NextTutorial();
        }
    }

    void CmdConfirm()
    {
        var order = new CmdOrder(nameof(TutorialUI), nameof(Confirm));
        PhotonManager.s.CmdCallFunction(order);
    }
    #endregion

    #region Private functions
    private void NextTutorial()
    {
        _tutorialIndex++;
        if (_tutorialIndex > _tutorialSprites.Count - 1)
        {
            OnTutorialComplete?.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            _tutorialImage.sprite = _tutorialSprites[_tutorialIndex];
            _checkImageList.Clear();
            _confirmBtn.gameObject.SetActive(true);
        }
    }
    #endregion
}
