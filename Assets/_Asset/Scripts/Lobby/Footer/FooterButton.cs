using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum FooterButtonStatus
{
    NORMAL,
    HIGHLIGHT,
    LOCKED,
    COMING_SOON,
}

public class FooterButton : MonoBehaviour
{
    public LobbyViewType type;
    public bool isComingSoon;
    public Button button;
    public TMP_Text txName;
    public GameObject objLocked;
    public Sprite spNormal;
    public Sprite spHighlight;
    public Sprite spLocked;
    public Sprite icNormal;
    public Sprite icSelect;
    public Image icon;
    public GameObject objNotify;

    protected FooterButtonStatus status;
    private Vector3 icondefaultPosition;
    private Tween iconTween;

    protected virtual void OnEnable()
    {
        CheckHandTutorial();
    }

    protected virtual void Awake()
    {
        button.onClick.AddListener(OnClick);
        if (icon != null)
        {
            icondefaultPosition = new Vector3(0, 0, 0);
        }
    }

    public virtual void CheckStatus(bool playanim = true)
    {
        if (isComingSoon)
        {
            status = FooterButtonStatus.COMING_SOON;
            button.image.sprite = spLocked;
            button.image.SetNativeSize();
            txName.gameObject.SetActive(false);
        }
        else
        {
            if (type == LobbyManager.Instance.curViewType)
            {
                status = FooterButtonStatus.HIGHLIGHT;
                txName.gameObject.SetActive(true);
                button.image.sprite = spHighlight;
                button.image.SetNativeSize();
                icon.sprite = icSelect;
                icon.SetNativeSize();
                if (playanim)
                    PlayAnim();
            }
            else
            {
                CheckLocked();
                txName.gameObject.SetActive(false);
                button.image.sprite = status == FooterButtonStatus.NORMAL ? spNormal : spLocked;
                button.image.SetNativeSize();
                icon.sprite = icNormal;
                icon.SetNativeSize();
                if (playanim)
                    StopAnim();
            }
        }
        objLocked.SetActive(status == FooterButtonStatus.LOCKED || status == FooterButtonStatus.COMING_SOON);
    }

    protected virtual void CheckLocked()
    {
        status = FooterButtonStatus.NORMAL;
    }

    public virtual void CheckNotify()
    {
        if (objNotify != null)
            objNotify.SetActive(false);
    }

    protected virtual void OnClick()
    {
        //AudioManager.Instance.PlaySfxClick();
        if (status == FooterButtonStatus.COMING_SOON)
        {
            UIManager.Instance.ShowToastMessage("Coming soon");
        }
        else if (status == FooterButtonStatus.LOCKED)
        {
        }
        else if (status == FooterButtonStatus.NORMAL)
        {
            LobbyManager.Instance.ShowView(type, false);
        }
    }

    public virtual void CheckHandTutorial()
    {

    }

    public virtual void PlayAnim()
    {
        if (iconTween != null) iconTween.Kill();

        icon.rectTransform.localPosition = icondefaultPosition;
        iconTween = icon.rectTransform
            .DOLocalMove(icondefaultPosition + new Vector3(0, 50, 0), 0.25f)
            .SetEase(Ease.OutBack);
    }

    public virtual void StopAnim()
    {
        if (iconTween != null) iconTween.Kill();

        iconTween = icon.rectTransform
            .DOLocalMove(icondefaultPosition, 0.2f)
            .SetEase(Ease.OutCubic);
    }
}

