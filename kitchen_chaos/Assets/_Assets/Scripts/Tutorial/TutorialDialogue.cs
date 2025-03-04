using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogue : MonoBehaviour
{
    public Action<int> OnDialogDone;

    private string[] _dialogues = new string[]
    {
        "Welcome to the tutorial!",
        "Lets make some <color=#FFC836>food</color>!",
        "We will learn how to make a  <color=#BCFF79>salad</color> first!",
        "Let pickup <sprite name=hand> a tomato <sprite name=tomato> and chop <sprite name=knife> it into tomato slices <sprite name=tomatoSlides>, watch out for the sharp knife.",
        "Then do the same with Cabbage <sprite name=cabbage>!",
        "Finally, combine two of them to make a <color=#BCFF79>salad</color>!. Just put one into the other.", // index 5
    };

    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private Button _nextButton, _backButton, _doneButton;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Image _tutorialImage;
    private int _currentDialogueIndex = 0;

    void Awake()
    {
        _nextButton.onClick.AddListener(NextDialogue);
        _backButton.onClick.AddListener(BackDialogue);
        _doneButton.onClick.AddListener(DoneDialogue);
        _tutorialImage.gameObject.SetActive(false);
        _doneButton.gameObject.SetActive(false);
    }

    public void StartDialogue()
    {
        _currentDialogueIndex = 0;
        ShowDialogue();
    }

    private void NextDialogue()
    {
        _currentDialogueIndex++;
        ShowDialogue();
    }
    private void BackDialogue()
    {
        _currentDialogueIndex--;
        ShowDialogue();
    }

    private void ShowDialogue()
    {
        _dialoguePanel.SetActive(true);
        _currentDialogueIndex = Mathf.Clamp(_currentDialogueIndex, 0, _dialogues.Length - 1);
        if (_currentDialogueIndex == _dialogues.Length - 1)
        {
            _doneButton.gameObject.SetActive(true);
            _tutorialImage.gameObject.SetActive(true);
        }
        else
        {
            _tutorialImage.gameObject.SetActive(false);
        }
        _tutorialText.text = _dialogues[_currentDialogueIndex];
    }

    private void DoneDialogue()
    {
        _dialoguePanel.SetActive(false);
        OnDialogDone?.Invoke(_currentDialogueIndex);
    }
}