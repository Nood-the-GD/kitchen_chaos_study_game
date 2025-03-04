using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOpenLink : MonoBehaviour
{
    private Button _button;
    [SerializeField] private string _link;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        Application.OpenURL(_link);
    }
}
