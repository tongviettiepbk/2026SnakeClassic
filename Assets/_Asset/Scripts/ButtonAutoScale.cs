using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAutoScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button btn;
    private Vector3 scale;

    [SerializeField] private float scales = 0.1f;

    private void Start()
    {
        scale = transform.localScale;
    }

    private void OnEnable()
    {
        if (!btn && GetComponent<Button>())
            btn = GetComponent<Button>();

        
        //btn.onClick.AddListener(Click);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn && btn.interactable)
            transform.localScale = new Vector3(scale.x + scales, scale.y + scales, scale.z + scales);

        try
        {
            AudioManager.Instance.PlaySfxClick();
        }
        catch { }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = scale;
    }

    //private void Click()
    //{
    //    AudioManager.Instance.PlaySfxClick();
    //}
}