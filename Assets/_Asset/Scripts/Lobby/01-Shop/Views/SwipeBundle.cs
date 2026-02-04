using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeBundle : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float swipeThreshold = 150f;

    private RectTransform rect;
    private Vector2 startDragPos;
    private Vector2 startAnchoredPos;
    private BoxBundle boxBundle;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        boxBundle = GetComponent<BoxBundle>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragPos = eventData.position;
        startAnchoredPos = rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.position.x - startDragPos.x;
        rect.anchoredPosition = startAnchoredPos + new Vector2(deltaX, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float deltaX = eventData.position.x - startDragPos.x;

        if (Mathf.Abs(deltaX) > swipeThreshold)
        {
            if (deltaX < 0)
                boxBundle.OnSwipeLeft();
            else
                boxBundle.OnSwipeRight();
        }
        else
        {
            rect.anchoredPosition = startAnchoredPos;
        }
    }
}