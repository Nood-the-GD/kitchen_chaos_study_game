using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FindFriendItemView : MonoBehaviour
{
    public Text txtName;
    UserData userData;

    public Button sentFriendReqeustButton;

    void Start(){
        sentFriendReqeustButton.onClick.AddListener(SentRequest);
    }

    

    public void SetData(UserData userData)
    {
        this.userData = userData;
        txtName.text = userData.username;
        var exit = SocialData.IsExistInMyRequest(userData.uid);
        sentFriendReqeustButton.gameObject.SetActive(!exit);
    }

    public async void SentRequest(){
        await LambdaAPI.SendFriendRequest(userData.uid);
        sentFriendReqeustButton.gameObject.SetActive(false);
    }
}
