using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FindFriendItemView : MonoBehaviour
{
    public Text txtName;
    UserData userData;
    public void SetData(UserData userData)
    {
        this.userData = userData;
        
    }

    public void sentRequest(){

    }
}
