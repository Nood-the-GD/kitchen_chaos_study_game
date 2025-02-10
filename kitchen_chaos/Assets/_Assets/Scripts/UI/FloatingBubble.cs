using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class FloatingBubble : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Snapping Settings")]
    [Tooltip("Speed at which the bubble snaps to the edge after dragging ends.")]
    public float snapSpeed = 10f;

    [Header("Drag Threshold Settings")]
    [Tooltip("Fraction of the canvas width required to drag the bubble for it to switch sides. e.g., 0.2 for 1/5 of the screen.")]
    public float dragThresholdFraction = 0.2f;

    [Header("Click Settings")]
    [Tooltip("If the pointer moves less than this many pixels, it is treated as a click.")]
    public float clickThreshold = 10f;
    [Tooltip("Event fired when the bubble is clicked.")]
    public UnityEvent onClickEvent;

    // Cached references.
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    // Drag tracking.
    private bool isDragging = false;
    private Vector2 dragOffset;
    private Vector2 pointerDownPos;

    // Record the bubble’s x‑position at the moment the drag starts.
    private float initialDragX;

    // Snapping.
    private bool isSnapping = false;
    private Vector2 targetAnchoredPosition;

    // Track which side the bubble is docked to.
    // True = docked left, false = docked right.
    private bool isDockedLeft = true;

    private void Awake()
    {
        // Cache our RectTransform.
        rectTransform = GetComponent<RectTransform>();

        // Assume the parent has a RectTransform (e.g., the Canvas).
        if (transform.parent != null)
        {
            canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        // Set initial docking side based on the current x position.
        isDockedLeft = rectTransform.anchoredPosition.x <= 0;
    }

    /// <summary>
    /// Called when the pointer is pressed down.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvasRectTransform == null)
            return;

        // Convert the pointer's screen position to the canvas' local space.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        // Calculate the offset so the bubble doesn’t “jump.”
        dragOffset = rectTransform.anchoredPosition - localPoint;
        pointerDownPos = eventData.position;

        // Record the bubble’s x‑position at the start of the drag.
        initialDragX = rectTransform.anchoredPosition.x;

        // Cancel any snapping in progress.
        isSnapping = false;
    }

    /// <summary>
    /// Called continuously as the bubble is dragged.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (canvasRectTransform == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        // Update the bubble's position.
        rectTransform.anchoredPosition = localPoint + dragOffset;
        isDragging = true;
    }

    /// <summary>
    /// Called when the drag ends. Decides whether to switch sides based on the horizontal displacement.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasRectTransform == null)
            return;

        isDragging = false;

        // Get canvas dimensions.
        float canvasWidth = canvasRectTransform.rect.width;
        float bubbleHalfWidth = rectTransform.rect.width / 2f;
        float leftBoundary = -canvasWidth / 2f;
        float rightBoundary = canvasRectTransform.rect.width / 2f;

        // Compute ideal docking positions.
        float leftDockX = leftBoundary + bubbleHalfWidth;
        float rightDockX = rightBoundary - bubbleHalfWidth;

        // Calculate the threshold (now adjustable via dragThresholdFraction).
        float threshold = canvasWidth * dragThresholdFraction;

        // Determine the horizontal displacement from where the drag started.
        float deltaX = rectTransform.anchoredPosition.x - initialDragX;

        if (isDockedLeft)
        {
            // If dragged to the right more than the threshold, switch to docking right.
            if (deltaX > threshold)
            {
                isDockedLeft = false;
                targetAnchoredPosition.x = rightDockX;
            }
            else
            {
                targetAnchoredPosition.x = leftDockX;
            }
        }
        else // Currently docked right.
        {
            // If dragged to the left more than the threshold, switch to docking left.
            if (deltaX < -threshold)
            {
                isDockedLeft = true;
                targetAnchoredPosition.x = leftDockX;
            }
            else
            {
                targetAnchoredPosition.x = rightDockX;
            }
        }

        // Clamp vertical position so the bubble remains visible.
        float bubbleHalfHeight = rectTransform.rect.height / 2f;
        float topBoundary = canvasRectTransform.rect.height / 2f;
        float bottomBoundary = -canvasRectTransform.rect.height / 2f;
        targetAnchoredPosition.y = Mathf.Clamp(
            rectTransform.anchoredPosition.y,
            bottomBoundary + bubbleHalfHeight,
            topBoundary - bubbleHalfHeight);

        // Start snapping.
        isSnapping = true;
    }

    /// <summary>
    /// Detects a click if the pointer movement is minimal.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Vector2.Distance(pointerDownPos, eventData.position) < clickThreshold)
        {
            onClickEvent?.Invoke();
        }
    }

    private void Update()
    {
        // Smoothly interpolate the bubble's position toward the target docking position.
        if (isSnapping)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition, targetAnchoredPosition, Time.deltaTime * snapSpeed);

            if (Vector2.Distance(rectTransform.anchoredPosition, targetAnchoredPosition) < 1f)
            {
                rectTransform.anchoredPosition = targetAnchoredPosition;
                isSnapping = false;
            }
        }
    }
}
