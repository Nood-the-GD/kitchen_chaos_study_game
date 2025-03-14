using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;

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
    
    // Reference to the scroll rect containing chat messages
    public ScrollRect chatScrollRect;
    
    // Maximum character limit for chat messages
    private const int MAX_CHAR_LIMIT = 80;
    public Text requestNuber;

    public static FriendPopup s => PopupController.s.GetActivePopup<FriendPopup>();

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
        
        // Set up character limit for the input field
        chatInputField.characterLimit = MAX_CHAR_LIMIT;
        
        Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ServerConnect.OnChatMessage += OnReciveChatMessage;
        SocialData.OnFriendAdded += OnFriendAdded;
        ServerConnect.OnSocialDataUpdate += OnSocialDataUpdate;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if(FloatingBubble.s != null)
            FloatingBubble.s.gameObject.SetActive(true);

        ServerConnect.OnChatMessage -= OnReciveChatMessage;
        SocialData.OnFriendAdded -= OnFriendAdded;
        ServerConnect.OnSocialDataUpdate -= OnSocialDataUpdate;
    }

    void OnReciveChatMessage(MessageData messageData, string converstationId){
        if(messageData == null){
            Debug.LogError("MessageData is null");
            return;
        }
        Debug.Log(messageData.content.ToString());
        if(currentChat.chatSummary.id == converstationId){
            AddMessage(messageData);
        }
    }

    void Init()
    {
        RefeshCountRequest();
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

    public void RefeshCountRequest(){
        Debug.Log("RefeshCountRequest: " + SocialData.mySocialData.otherRequest.Count);
        requestNuber.text = SocialData.mySocialData.otherRequest.Count.ToString();
    }

    async void SendMessage() 
    {
        // Check if the input field is empty
        if (string.IsNullOrWhiteSpace(chatInputField.text))
            return;
            
        if(currentChat.chatSummary == null){
            Debug.Log("Create chat with user: " + currentChat.otherUid);
            await LambdaAPI.CreateChatMessage(currentChat.otherUid, chatInputField.text);
        }else{
            Debug.Log("Send message to chat: " + currentChat.otherUid);
            await LambdaAPI.SendChatMessage(currentChat.chatSummary.id, chatInputField.text);
        }
        
        // Clear the input field after sending the message
        chatInputField.text = "";
        
        // Set focus back to the input field
        chatInputField.ActivateInputField();

    }

    void OnClick(FriendChatItemView friendChatItemView)
    {
        currentChat = friendChatItemView;
        SetActiveChatPanel(true);
        ReloadChatUI();
    }

    public async void ReloadChatUI(){
        // Load chat UI
        chatItemViewRef.gameObject.SetActive(false);
       
        var child = chatItemViewRef.transform.parent.childCount;
        for(int i = 1; i < child; i++){
            Destroy(chatItemViewRef.transform.parent.GetChild(i).gameObject);
        }

        if(currentChat.chatSummary == null){
            return;
        }

        var convo = await ConversationData.LoadConversationDataAsync(currentChat.chatSummary.id);

        if(convo == null){
            Debug.LogError("Conversation not found");
            return;
        }
        convo.UpdateTimeConverstaiton();

        //sort the time
        var sortedMessages = convo.messages.OrderBy(x => x.Value.timestamp).ToList();

        foreach(var i in sortedMessages){
            Debug.Log("Message: " + i.Value.content.ToString());
            AddMessage(i.Value, false); // Don't scroll for each message during initialization
        }
        
        // Wait a frame to ensure all messages are properly laid out
        await System.Threading.Tasks.Task.Delay(50);
        
        // Scroll to bottom after all messages are loaded
        ScrollToBottomImmediate();
    }

    void AddMessage(MessageData messageData, bool scrollToBottom = true){
        ChatItemView chatItemView = Instantiate(chatItemViewRef, chatItemViewRef.transform.parent);
        chatItemView.gameObject.SetActive(true);
        chatItemView.SetData(messageData);
        
        // Scroll to the bottom with animation after adding a new message, if requested
        if (scrollToBottom)
        {
            // Ensure the chat panel is active before attempting to scroll
            if (messagePanel.gameObject.activeInHierarchy)
            {
                StartCoroutine(ScrollToBottomAnimated());
            }
        }
    }
    
    /// <summary>
    /// Coroutine to scroll to the bottom of the chat with animation
    /// </summary>
    private IEnumerator ScrollToBottomAnimated()
    {
        // Wait for the end of frame to ensure the UI has been updated
        yield return new WaitForEndOfFrame();
        
        // Wait one more frame to ensure content size has been recalculated
        yield return null;
        
        // Make sure we have a valid scroll rect
        if (chatScrollRect != null && chatScrollRect.content != null)
        {
            // Force layout rebuild to ensure content size is correct
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatScrollRect.content);
            Canvas.ForceUpdateCanvases();
            
            // Wait another frame after layout rebuild
            yield return null;
            
            // Ensure content size fitter has updated if present
            ContentSizeFitter sizeFitter = chatScrollRect.content.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                sizeFitter.enabled = false;
                yield return null;
                sizeFitter.enabled = true;
                yield return null;
            }
            
            // Force another canvas update
            Canvas.ForceUpdateCanvases();
            
            // Animate scrolling to the bottom
            float startPos = chatScrollRect.verticalNormalizedPosition;
            float endPos = 0f; // 0 is bottom, 1 is top for vertical scroll
            float elapsedTime = 0f;
            float scrollDuration = 0.25f; // Duration of scroll animation
            
            while (elapsedTime < scrollDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / scrollDuration);
                chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            // Ensure we're exactly at the bottom
            chatScrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
            
            // Debug log to confirm scrolling completed
            Debug.Log("Animated scroll to bottom completed");
        }
        else
        {
            Debug.LogWarning("Cannot scroll: chatScrollRect or its content is null");
        }
    }
    
    /// <summary>
    /// Immediately scrolls to the bottom of the chat without animation
    /// </summary>
    private void ScrollToBottomImmediate()
    {
        if (chatScrollRect == null || chatScrollRect.content == null)
        {
            Debug.LogWarning("Cannot scroll: chatScrollRect or its content is null");
            return;
        }
        
        // Force layout rebuild to ensure content size is correct
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatScrollRect.content);
        Canvas.ForceUpdateCanvases();
        
        // Check for ContentSizeFitter and refresh it if present
        ContentSizeFitter sizeFitter = chatScrollRect.content.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            sizeFitter.enabled = false;
            sizeFitter.enabled = true;
        }
        
        // Force another canvas update
        Canvas.ForceUpdateCanvases();
        
        // Set scroll position to bottom
        chatScrollRect.verticalNormalizedPosition = 0f;
        
        // Force final canvas update
        Canvas.ForceUpdateCanvases();
        
        // Debug log to confirm scrolling completed
        Debug.Log("Immediate scroll to bottom completed");
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

    // Method called when a new friend is added
    private void OnFriendAdded(string friendUid)
    {
        // Refresh the friend list
        RefreshFriendList();
        RefeshCountRequest();
    }

    // Method to refresh the friend list
    public void RefreshFriendList()
    {
        // Clear existing friend items except the prefab
        Transform parent = pref.transform.parent;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != pref.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Reinstantiate friend items from the updated friend list
        var friends = SocialData.mySocialData.friends;
        foreach (var friend in friends)
        {
            FriendChatItemView friendItem = Instantiate(pref, parent);
            friendItem.gameObject.SetActive(true);
            friendItem.SetData(friend, OnClick);
        }
    }

    // New method to handle social data updates
    private void OnSocialDataUpdate()
    {
        // Update the friend request number
        requestNuber.text = SocialData.mySocialData.otherRequest.Count.ToString();
        
        // Refresh the friend list UI
        RefreshFriendList();

        
        // If we have an active chat, reload it to reflect any changes
        if (currentChat != null && messagePanel.gameObject.activeInHierarchy)
        {
            ReloadChatUI();
        }
    }
}
