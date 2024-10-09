using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class Table : BaseCounter, IPlaceable
{
    [SerializeField] private List<Transform> _chairTransforms;
    [SerializeField] private Material _activeChairMaterial, _inactiveChairMaterial;
    private Collider[] _colliders;

    public Transform Transform => this.transform;
    public int ChairNumber => _chairTransforms.Count;
    public List<Transform> Chairs => _chairTransforms;

    protected override void Awake()
    {
        base.Awake();
        _colliders = this.GetComponentsInChildren<Collider>();
    }

    public void StartPlacing()
    {
        PlaceObjectManager.s.StartPlacingObject(this);
        _colliders.ToList().ForEach(collider => collider.enabled = false);
        TableManager.s.RemoveTable(this);

        // Play disappear animation for each chair
        foreach (var chairTransform in _chairTransforms)
        {
            Vector3 originalRotation = chairTransform.rotation.eulerAngles;
            chairTransform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InOutBack)
                .OnStart(() => chairTransform.DORotate(new Vector3(0, 360, 0), 0.3f, RotateMode.FastBeyond360)
                    .OnComplete(() => chairTransform.rotation = Quaternion.Euler(originalRotation)));
        }
    }

    public void PlaceObject()
    {
        _colliders.ToList().ForEach(collider => collider.enabled = true);
        TableManager.s.AddTable(this);

        // Play appear animation for each chair
        foreach (var chairTransform in _chairTransforms)
        {
            chairTransform.gameObject.SetActive(true);
            Vector3 originalPosition = chairTransform.position;
            Vector3 originalRotation = chairTransform.rotation.eulerAngles;

            chairTransform.localScale = Vector3.zero;
            chairTransform.position += Vector3.up * 2f; // Start 2 units above

            Sequence chairSequence = DOTween.Sequence();
            chairSequence.Append(chairTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce))
                         .Join(chairTransform.DOMove(originalPosition, 0.5f).SetEase(Ease.OutQuad))
                         .Join(chairTransform.DORotate(originalRotation, 0.5f, RotateMode.FastBeyond360));
        }
    }

    public void ActiveChair()
    {
        foreach (var chair in _chairTransforms)
        {
            chair.GetComponent<Renderer>().material = _activeChairMaterial;
            // Play activation animation for each chair
            chair.localScale = Vector3.one * 0.8f; // Start slightly smaller
            chair.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutElastic)
                .OnComplete(() =>
                {
                    chair.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.InOutSine);
                });
        }
    }
}







