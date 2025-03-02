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

    private void CreateAnimationTween()
    {
        _navigationArrow.transform.DOMove(_navigationArrow.transform.position + Vector3.up * 0.3f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        _navigationArrow.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    public void ShowNavigationArrow(Transform target)
    {
        var position = target.position;
        position.y += 3f;
        _navigationArrow.transform.position = position;
        _navigationArrow.gameObject.SetActive(true);
        _navigationArrow.transform.DOKill();
        CreateAnimationTween();
    }
}