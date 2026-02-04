using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BaseButtonLobby : MonoBehaviour
{
    protected Button button;
    protected TMP_Text txLabel;
    protected GameObject objNotify;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);

        objNotify = transform.Find("Notify").gameObject;
    }

    public virtual void CheckActive()
    {
        gameObject.SetActive(false);
    }

    public virtual void CheckNotify()
    {
        if (objNotify != null)
            objNotify.SetActive(false);
    }

    public virtual void OnClick()
    {
        //AudioManager.Instance.PlaySfxClick();
    }
}
