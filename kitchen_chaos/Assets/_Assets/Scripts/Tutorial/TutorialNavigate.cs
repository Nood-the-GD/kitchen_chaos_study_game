using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialNavigate : Singleton<TutorialNavigate>
{
    [SerializeField] private GameObject _navigationArrow;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ShowNavigationArrow(Transform target)
    {
        var position = target.position;
        position.y += 3f;
        _navigationArrow.transform.DOMove(position, 1f).SetEase(Ease.InOutBack);
        _navigationArrow.gameObject.SetActive(true);
    }
}