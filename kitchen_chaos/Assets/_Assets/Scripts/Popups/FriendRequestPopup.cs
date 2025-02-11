using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // For DOTween animation

public class FriendRequestPopup : BasePopup<FriendRequestPopup>
{
    // This prefab should have a FriendRequestItemView component attached.
    public FriendRequestItemView friendRequestItemView;

    void Start()
    {
        friendRequestItemView.gameObject.SetActive(false);
        PopulateFriendRequests();
    }

    /// <summary>
    /// Populates the friend request popup by instantiating a UI item for each pending friend request.
    /// </summary>
    void PopulateFriendRequests()
    {
        // Make sure we have valid SocialData with friend requests.
        if (SocialData.mySocialData == null || SocialData.mySocialData.otherRequest == null)
        {
            Debug.LogWarning("No friend request data available.");
            return;
        }

        // Get the friend requests dictionary (key: friend's UID, value: request time in UTC).
        var requests = SocialData.mySocialData.otherRequest;

        // Use the parent of the template as the container for all instantiated items.
        Transform parent = friendRequestItemView.transform.parent;

        // Optionally clear out any previous friend request items (keeping the template intact).
        List<Transform> childrenToRemove = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.gameObject != friendRequestItemView.gameObject)
            {
                childrenToRemove.Add(child);
            }
        }
        foreach (var child in childrenToRemove)
        {
            Destroy(child.gameObject);
        }

        // Create a list to hold instantiated friend request items (for later animation).
        List<FriendRequestItemView> requestItems = new List<FriendRequestItemView>();

        // For each pending friend request, instantiate a new UI item.
        foreach (KeyValuePair<string, long> request in requests)
        {
            string friendUid = request.Key;
            long requestTime = request.Value;

            // Instantiate a copy of the template.
            FriendRequestItemView item = Instantiate(friendRequestItemView, parent);

            // Make sure the instantiated item is visible.
            item.gameObject.SetActive(true);

            // Set its initial scale to zero so we can animate it in.
            item.transform.localScale = Vector3.zero;

            // Pass the data (friend's UID and request time) to the item.
            // (Implement SetData in your FriendRequestItemView script accordingly.)
            item.SetData(friendUid);

            // Add the item to the list for sequential animation.
            requestItems.Add(item);
        }

        // Animate the friend request items if there are any.
        if (requestItems.Count > 0)
        {
            AnimateFriendRequestItemsSequentially(requestItems);
        }

        // Hide the original template so it remains only as a prefab.
        friendRequestItemView.gameObject.SetActive(false);
    }

    /// <summary>
    /// Animates the friend request items sequentially using DOTween.
    /// Each item scales from zero to its normal size.
    /// </summary>
    /// <param name="items">List of friend request item views to animate.</param>
    void AnimateFriendRequestItemsSequentially(List<FriendRequestItemView> items)
    {
        // Create a DOTween sequence to chain the animations.
        Sequence seq = DOTween.Sequence();

        foreach (var item in items)
        {
            // Append a tween to scale the item from zero to one over 0.1 seconds.
            seq.Append(item.transform.DOScale(Vector3.one, 0.1f));

            // Append a short delay before animating the next item.
            seq.AppendInterval(0.01f);
        }
    }
}
