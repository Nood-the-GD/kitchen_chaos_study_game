using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

public class FriendPopup : BasePopup<FriendPopup>
{
    public Button findFriendButton;
    public Button friendRequestButton;
    public FriendChatItemView pref;
    
    // The friend panel (default: posX = 0, width = 1750)
    public RectTransform friendPanel;
    
    // The chat (message) panel – initially not active.
    public RectTransform messagePanel;
    public float animDur = 0.3f;
    
    private FriendChatItemView currentChat;
    public InputField chatInputField;
    public Button sendMessageButton;
    public ChatItemView chatItemViewRef;
    

    [Button]
    void LogSocialData(){
        SocialData.Log();
    }

    void Start()
    {

        
        sendMessageButton.onClick.AddListener(SendMessage);
        findFriendButton.onClick.AddListener(OnFindFriendClick);
        friendRequestButton.onClick.AddListener(OnFriendRequestClick);
        
        // Ensure the friend panel is set to its default values.
        friendPanel.anchoredPosition = new Vector2(0, friendPanel.anchoredPosition.y);
        friendPanel.sizeDelta = new Vector2(1750f, friendPanel.sizeDelta.y);
        
        // Set the chat (message) panel inactive on start.
        messagePanel.gameObject.SetActive(false);
        

       
        Init();
        
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        ServerConnect.OnChatMessage += OnReciveChatMessage;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if(FloatingBubble.s != null)
            FloatingBubble.s.gameObject.SetActive(true);

        ServerConnect.OnChatMessage -= OnReciveChatMessage;

    }

    void OnReciveChatMessage(MessageData messageData){
        Debug.Log(messageData.content.ToString());
        AddMessage(messageData);
    }


    void Init()
    {
        // Hide the prefab by default
        pref.gameObject.SetActive(false);
        
        // Instantiate friend items from the friend list
        var friends = SocialData.mySocialData.friends;
        foreach (var friend in friends)
        {
            FriendChatItemView friendItem = Instantiate(pref, pref.transform.parent);
            friendItem.gameObject.SetActive(true);
            friendItem.SetData(friend, OnClick);
        }
    }


    async void SendMessage() 
    {
        if(currentChat.chatSummary == null){
            Debug.Log("Create chat with user: " + currentChat.otherUid);
            await LambdaAPI.CreateChatMessage(currentChat.otherUid, chatInputField.text);
        }else{

            Debug.Log("Send message to chat: " + currentChat.otherUid);
            await LambdaAPI.SendChatMessage(currentChat.otherUid, chatInputField.text);
        }
    }

    void OnClick(FriendChatItemView friendChatItemView)
    {
        currentChat = friendChatItemView;
        SetActiveChatPanel(true);
        ReloadChatUI();
    }

    async void ReloadChatUI(){
        // Load chat UI
        chatItemViewRef.gameObject.SetActive(false);
        var convo = await ConversationData.LoadConversationDataAsync(currentChat.chatSummary.id);
        var child = chatItemViewRef.transform.parent.childCount;
        for(int i = 1; i < child; i++){
            Destroy(chatItemViewRef.transform.parent.GetChild(i).gameObject);
        }

        if(convo == null){
            return;
        }

        foreach(var i in convo.messageData){
            AddMessage(i);
        }
    }

    void AddMessage(MessageData messageData){
        ChatItemView chatItemView = Instantiate(chatItemViewRef, chatItemViewRef.transform.parent);
        chatItemView.gameObject.SetActive(true);
        chatItemView.SetData(messageData);
    }




    /// <summary>
    /// Animates the friend and chat panels.
    /// When active is true:
    ///   - The message panel is activated and scales in.
    ///   - The friend panel moves to posX = -193 and its width changes to 1045.505.
    /// When active is false:
    ///   - The friend panel reverts to posX = 0 and width = 1750.
    ///   - The message panel scales down and is deactivated.
    /// </summary>
    public void SetActiveChatPanel(bool active)
    {
        float duration = animDur; // Duration for the animations

        if (active)
        {
            // Activate and animate the message panel scaling in.
            messagePanel.gameObject.SetActive(true);
            messagePanel.localScale = Vector3.zero;
            DOTween.To(
                () => messagePanel.localScale,
                x => messagePanel.localScale = x,
                Vector3.one,
                duration
            ).SetEase(Ease.OutBack);

            // Animate the friendPanel's anchored X position from 0 to -193.
            DOTween.To(
                () => friendPanel.anchoredPosition.x,
                x =>
                {
                    Vector2 pos = friendPanel.anchoredPosition;
                    pos.x = x;
                    friendPanel.anchoredPosition = pos;
                },
                -193f,
                duration
            ).SetEase(Ease.OutQuad);

            // Animate the friendPanel's width (sizeDelta.x) from 1750 to 1045.505.
            DOTween.To(
                () => friendPanel.sizeDelta.x,
                x =>
                {
                    Vector2 size = friendPanel.sizeDelta;
                    size.x = x;
                    friendPanel.sizeDelta = size;
                },
                1045.505f,
                duration
            ).SetEase(Ease.OutQuad);
        }
        else
        {
            // Revert friendPanel back to its default position and width.
            DOTween.To(
                () => friendPanel.anchoredPosition.x,
                x =>
                {
                    Vector2 pos = friendPanel.anchoredPosition;
                    pos.x = x;
                    friendPanel.anchoredPosition = pos;
                },
                0f,
                duration
            ).SetEase(Ease.OutQuad);

            DOTween.To(
                () => friendPanel.sizeDelta.x,
                x =>
                {
                    Vector2 size = friendPanel.sizeDelta;
                    size.x = x;
                    friendPanel.sizeDelta = size;
                },
                1750f,
                duration
            ).SetEase(Ease.OutQuad);

            // Animate the message panel scaling down, then deactivate it.
            DOTween.To(
                () => messagePanel.localScale,
                x => messagePanel.localScale = x,
                Vector3.zero,
                duration
            ).SetEase(Ease.InBack)
             .OnComplete(() => { messagePanel.gameObject.SetActive(false); });
        }
    }

    void OnFindFriendClick()
    {
        FindFriendPopup.ShowPopup();
    }

    void OnFriendRequestClick()
    {
        FriendRequestPopup.ShowPopup();
    }

    

}
