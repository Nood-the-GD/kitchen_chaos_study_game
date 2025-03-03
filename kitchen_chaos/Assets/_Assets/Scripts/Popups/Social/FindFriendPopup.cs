using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // DOTween namespace

public class FindFriendPopup : BasePopup<FindFriendPopup>
{   
    public InputField textField;
    public Button btnSearch;
    public FindFriendItemView findFriendItemView;

    void Start()
    {
        btnSearch.onClick.AddListener(Search);
        // Hide the prefab template initially
        findFriendItemView.gameObject.SetActive(false);
    }

    public async void Search()
    {
        // Clear previous search results (all children except the prefab template)
        Transform parent = findFriendItemView.transform.parent;
        List<Transform> childrenToRemove = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.gameObject != findFriendItemView.gameObject)
            {
                childrenToRemove.Add(child);
            }
        }
        foreach (Transform child in childrenToRemove)
        {
            Destroy(child.gameObject);
        }

        // Perform the search and instantiate new friend items
        var p = await LambdaAPI.FindFriend(textField.text);
        // List to hold the instantiated friend items for sequential animation.
        List<FindFriendItemView> friendItems = new List<FindFriendItemView>();

        p.jToken.ToObject<List<UserData>>().ForEach((userData) =>
        {
            if(!userData.IsMine()){
                // Instantiate the prefab as a new item under the same parent
                var item = Instantiate(findFriendItemView, findFriendItemView.transform.parent);
                item.gameObject.SetActive(true);

                // Set the initial scale to zero so it can be animated in.
                item.transform.localScale = Vector3.zero;

                // Set the friend data (e.g., name, image, etc.)
                item.SetData(userData);

                // Add the item to the list for later animation
                friendItems.Add(item);
            }
        });

        // Animate friend items sequentially using DOTween
        AnimateFriendItemsSequentially(friendItems);

        // Keep the prefab template hidden
        findFriendItemView.gameObject.SetActive(false);
    }

    /// <summary>
    /// Animates a list of friend items sequentially using DOTween,
    /// adding a short delay between each item's animation.
    /// </summary>
    void AnimateFriendItemsSequentially(List<FindFriendItemView> items)
    {
        // Create a DOTween sequence to chain animations one after the other.
        Sequence seq = DOTween.Sequence();

        foreach (var item in items)
        {
            // Append a tween to scale the item from zero to one over 0.12 seconds
            seq.Append(item.transform.DOScale(Vector3.one, 0.1f));

            // Append an interval (delay) of 0.02 seconds before the next animation
            seq.AppendInterval(0.01f);
        }
    }
}
