using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatItemView : MonoBehaviour
{   
    public Text userName;
    public Text message;
    public Text time;
    public GameObject activeStatus;
    public 
    
    // Start is called before the first frame update
    void Start()
    {

        // Example: Creating a MessageData instance.
        MessageData message = new MessageData("Hello, Unity!", 123456789, "User123");

        // Converting the MessageData instance to a JSON string.
        string json = message.ToJson();
        Debug.Log("MessageData as JSON: " + json);

        // Parsing the JSON back into a MessageData instance.
        MessageData parsedMessage = MessageData.FromJson(json);
        Debug.Log("Parsed MessageData: " + parsedMessage.ToString());

        // Using the CopyWith method to create a modified copy.
        MessageData modifiedMessage = message.CopyWith(content: "Hello again!");
        Debug.Log("Modified MessageData: " + modifiedMessage.ToString());

        // Creating a MessageDataChannel instance.
        MessageDataChannel channel = new MessageDataChannel("General", message);
        Debug.Log("MessageDataChannel: channel = " + channel.channel + ", message = " + channel.messageData.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        // Your update logic here.
    }
}
