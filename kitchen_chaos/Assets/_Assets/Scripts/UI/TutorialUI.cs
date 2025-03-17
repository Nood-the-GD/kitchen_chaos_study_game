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
    [SerializeField] private Image _tutorialImage;
    [SerializeField] private Image _checkImage;
    [SerializeField] private Transform _checkImageHolder;

    private List<Sprite> _tutorialSprites = new List<Sprite>();
    private List<Image> _checkImageList = new List<Image>();
    private int _tutorialIndex = 0;
    #endregion

    #region Unity functions
    private void Start()
    {
        _checkImage.gameObject.SetActive(false);
        _confirmBtn.onClick.AddListener(() =>
        {
            _confirmBtn.gameObject.SetActive(false);
            Confirm();
        });
        _tutorialSprites = GameData.s.GetTutorialImages(GameManager.levelId);
        _tutorialIndex = 0;
        if (_tutorialSprites.Count == 0 || TutorialData.Instance.GetSkipTutorialNumber() == 2)
        {
            _tutorialImage.gameObject.SetActive(false);
            _confirmBtn.gameObject.SetActive(false);
            StartCoroutine(DelayEvent()); // Make sure all script has subscribe to event
        }
        else
        {
            _tutorialImage.sprite = _tutorialSprites[_tutorialIndex];
        }
    }
    void OnDestroy()
    {
        if (PhotonManager.s == null) return;
    }
    IEnumerator DelayEvent()
    {
        yield return new WaitForSeconds(0.5f);
        OnTutorialComplete?.Invoke();
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Multiplayer functions
    private void Confirm()
    {
        TutorialData.Instance.CmdConfirmTutorial();
        Image checkImage = Instantiate(_checkImage, _checkImageHolder);
        checkImage.gameObject.SetActive(true);
        _checkImageList.Add(checkImage);

        if (TutorialData.Instance.GetConfirmTutorialNumber() >= 2)
        {
            NextTutorial();
        }
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
            foreach (Image img in _checkImageList)
            {
                Destroy(img.gameObject);
            }
            _checkImageList.Clear();
            _confirmBtn.gameObject.SetActive(true);
            TutorialData.Instance.ClearConfirmTutorial();
        }
    }
    #endregion
}
