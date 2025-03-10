using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Dropdown))]
public class ServerPingDropdown : MonoBehaviourPunCallbacks
{
    public Dropdown serverDropdown;
    private void Start()
    {
        serverDropdown.onValueChanged.AddListener((int index) =>
        {
            var region = PhotonManager.s.allRegionPing[index].region;
            if(UserSetting.regionSelected != region){
                PhotonManager.Disconnect();
                PhotonManager.ConnectToServer(region);
            }
            UserSetting.regionSelected = region;
            
        });
        StartCoroutine(UpdateTheList());

    }
    IEnumerator UpdateTheList()
    {
        while (true)
        {

            var pings = PhotonManager.s.allRegionPing;
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (var item in pings)
            {
                var text = item.region + " ";
                if (item.ping < 100)
                {
                    text += WrapTextInColorTag(item.ping + "ms", "green");
                }
                else if (item.ping < 170)
                {
                    text += WrapTextInColorTag(item.ping + "ms", "yellow");
                }
                else
                {
                    text += WrapTextInColorTag(item.ping + "ms", "red");
                }
                options.Add(new Dropdown.OptionData(text));

            }

            serverDropdown.options = options;
            serverDropdown.value = pings.FindIndex(x => x.region == UserSetting.regionSelected);
            serverDropdown.RefreshShownValue();
            yield return new WaitForSeconds(2);

            PhotonManager.s.RefeshPing();
        }
    }

    // Function to wrap text in a color tag
    public static string WrapTextInColorTag(string text, string color)
    {
        switch (color.ToLower())
        {
            case "yellow":
                return $"<color=yellow>{text}</color>";
            case "green":
                return $"<color=#00FF00>{text}</color>";
            case "red":
                return $"<color=red>{text}</color>";
            default:
                return text; // If the color is not recognized, return the original text
        }
    }
}
