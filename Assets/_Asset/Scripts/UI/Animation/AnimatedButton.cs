using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isDecrease;
    private bool isNormalize;

    private RectTransform rectTransform;
    private float originalScale = 1f;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale.x;
    }

    protected virtual void OnEnable()
    {
        rectTransform.localScale = Vector3.one * originalScale;
    }

    protected virtual void Update()
    {
        if (isDecrease)
        {
            rectTransform.localScale = Vector3.MoveTowards(rectTransform.localScale, Vector3.one * originalScale * 0.9f, 10f * Time.deltaTime);
        }
        else if (isNormalize)
        {
            rectTransform.localScale = Vector3.MoveTowards(rectTransform.localScale, Vector3.one * originalScale, 10f * Time.deltaTime);
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.localScale = Vector3.one * originalScale;
        isDecrease = true;
        isNormalize = false;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isDecrease = false;
        isNormalize = true;
    }

    public virtual void Highlight(bool isOn) { }
}
