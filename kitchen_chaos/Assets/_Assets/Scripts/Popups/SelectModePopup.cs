using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModePopup : BasePopup<SelectModePopup>
{
    public void RandomMatch(){

    }
    public void FindRoom(){
        FindRoomPopup.ShowPopup();
    }

    public void CreateRoom(){
        CreateRoomPopup.ShowPopup();
    }

}
