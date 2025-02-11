using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FriendPopup : BasePopup<FriendPopup>
{
    public Button findFriendButton;
    public Button friendRequestButton;

    void Start(){
        findFriendButton.onClick.AddListener(OnFindFriendClick);
        friendRequestButton.onClick.AddListener(OnFriendRequestClick);
    }

    void OnFindFriendClick(){
        FindFriendPopup.ShowPopup();
    }

    void OnFriendRequestClick(){
        FriendRequestPopup.ShowPopup();
    }

    protected override void OnDisable(){
        base.OnDisable();
        FloatingBubble.s.gameObject.SetActive(true);
    }
}
