using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipRewards : MonoBehaviour
{
    public RectTransform rect;
    public RectTransform contentRect;
    public Image icon;
    public TMP_Text txTitle;
    public TMP_Text txDescription;
    public GameObject objArrow;

    private bool animateDone;
    private Vector3 screenPoint;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (animateDone && gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Show(RewardData data, RectTransform objectRect, TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft)
    {
        if (data != null)
        {
            Show(data.type, objectRect, textAlignment);
        }
        else
        {
            Debug.LogError("ShowTooltips NULL");
            gameObject.SetActive(false);
        }
    }

    public bool Show(ItemType type, RectTransform objectRect, TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft)
    {
        ItemData itData = GameData.staticData.items.GetData(type);
        if (itData != null)
        {
            string desc = itData.GetDescription();

            return Show(itData.icon, itData.GetName(), desc, objectRect, textAlignment);
        }

        Debug.LogError("ShowTooltips NULL");
        gameObject.SetActive(false);
        return false;
    }

    public bool Show(Sprite img, string title, string description, RectTransform objectRect, TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft)
    {
        Canvas canvas = objectRect.root.GetComponent<Canvas>();
        if (canvas == null)
        {
            gameObject.SetActive(false);
            return false;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            screenPoint = Camera.main.WorldToScreenPoint(objectRect.position);
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            screenPoint = objectRect.position;
        }

        transform.position = screenPoint;

        animateDone = false;
        icon.sprite = img;
        txTitle.text = title;
        txTitle.alignment = textAlignment;
        txDescription.text = description;
        txDescription.alignment = textAlignment;

        icon.gameObject.SetActive(img != null);
        txTitle.gameObject.SetActive(!string.IsNullOrEmpty(title));
        icon.transform.parent.gameObject.SetActive(icon.gameObject.activeSelf || txTitle.gameObject.activeSelf);

        UIManager.Instance.StartActionEndOfFrame(() =>
        {
            contentRect.gameObject.SetActive(false);
            contentRect.gameObject.SetActive(true);
            Resize();
        });

        gameObject.SetActive(false);
        gameObject.SetActive(true);

        return true;
    }

    private void Resize()
    {
        float screenWidth = 720f;
        float screenHeight = 1280f;

        UIManager.Instance.StartActionEndOfFrame(() =>
        {
            Vector3 vRect = rect.anchoredPosition;

            float halfContentWidth = contentRect.rect.width / 2f;
            float halfContentHeight = contentRect.rect.height / 2f;
            float maxX = screenWidth / 2f;
            float maxY = screenHeight / 2f;
            float margin = 10f;

            if (vRect.x < 0 && Mathf.Abs(vRect.x) + halfContentWidth > maxX)
            {
                vRect.x = -(maxX - halfContentWidth - margin);
            }

            if (vRect.x > 0 && vRect.x + halfContentWidth > maxX)
            {
                vRect.x = maxX - halfContentWidth - margin;
            }

            if (vRect.y > 0 && vRect.y + halfContentHeight > maxY)
            {
                vRect.y = maxY - halfContentHeight - margin;
            }

            if (vRect.y < 0 && Mathf.Abs(vRect.y) + halfContentHeight > maxY)
            {
                vRect.y = -(maxY - halfContentHeight - margin);
            }

            rect.anchoredPosition = vRect;

            Vector2 vArrow = objArrow.transform.position;
            vArrow.x = screenPoint.x;
            objArrow.transform.position = vArrow;

            animateDone = true;
        });
    }
}