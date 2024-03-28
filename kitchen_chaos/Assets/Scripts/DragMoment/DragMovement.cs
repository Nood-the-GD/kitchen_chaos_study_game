using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 pointerPosition;
    private Vector2 playerInput;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 moveOffset = eventData.position - pointerPosition;
        playerInput = moveOffset.normalized;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        playerInput = Vector2.zero;
    }

    public Vector2 GetPlayerInput()
    {
        return this.playerInput;
    }
}
